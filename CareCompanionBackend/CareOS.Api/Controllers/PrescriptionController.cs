using CareOS.Api.DTOs;
using CareOS.Api.Services;
using CareOS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILocalPrescriptionScannerService _localScannerService;
        private readonly ILogger<PrescriptionController> _logger;
        private readonly IAiService _aiService;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            ILocalPrescriptionScannerService localScannerService,
            ILogger<PrescriptionController> logger,
            IAiService aiService)
        {
            _prescriptionService = prescriptionService;
            _localScannerService = localScannerService;
            _logger = logger;
            _aiService = aiService;
        }

        // POST: api/Prescription/upload
        [HttpPost("upload")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> UploadPrescription([FromBody] UploadPrescriptionDto request)
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ensure elder is uploading for themselves
            if (request.ElderId != elderId)
            {
                return Unauthorized(new { message = "You can only upload prescriptions for yourself" });
            }

            var result = await _prescriptionService.UploadPrescriptionAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Upload prescription with local OCR scan
        /// </summary>
      [HttpPost("upload-and-scan")]
     [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> UploadAndScanPrescription([FromBody] UploadAndScanDto request)
        {
   var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
     return Unauthorized(new { message = "Invalid token" });

     // Local OCR scan کریں
 var scanResult = await _localScannerService.ScanPrescriptionLocallyAsync(request.Base64Image);

          if (!scanResult.Success)
                return BadRequest(scanResult);

    // Prescription بنائیں
    var prescription = new Prescription
   {
      ElderId = elderId,
       ElderName = request.ElderName ?? "Unknown",
         Base64Image = request.Base64Image,
       Analysis = scanResult.Data!,
       Status = "PENDING",
   UploadedAt = DateTime.UtcNow
      };

            // Database میں save کریں
      var result = await _prescriptionService.CreatePrescriptionAsync(prescription);

return Ok(new
            {
          success = true,
     message = "Prescription scanned and submitted for approval",
          prescriptionId = prescription.Id,
       analysis = scanResult.Data
   });
        }

        /// <summary>
        /// Upload prescription with choice: OCR or Gemini
        /// </summary>
   [HttpPost("upload-with-choice")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> UploadWithChoice([FromBody] UploadWithChoiceDto request)
   {
    var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 _logger.LogInformation($"📤 Upload request from Elder ID: {elderId}");

          if (string.IsNullOrEmpty(elderId))
        return Unauthorized(new { message = "Invalid token" });

      if (request.ScanMethod != "OCR" && request.ScanMethod != "GEMINI")
  return BadRequest(new { message = "Invalid scan method. Use 'OCR' or 'GEMINI'" });

         ApiResponse<PrescriptionAnalysisDto> scanResult;

    // Choose scanning method
            if (request.ScanMethod == "OCR")
 {
      _logger.LogInformation("📁 Using Local OCR scanning");
     scanResult = await _localScannerService.ScanPrescriptionLocallyAsync(request.Base64Image);
    }
   else // GEMINI
      {
  _logger.LogInformation("🤖 Using Gemini AI scanning");
   scanResult = await _aiService.AnalyzePrescriptionAsync(request.Base64Image);
  }

if (!scanResult.Success)
   {
      _logger.LogError($"❌ Scan failed: {scanResult.Message}");
  return BadRequest(scanResult);
          }

 _logger.LogInformation("✅ Scan successful, creating prescription...");

  // Create prescription record
         var prescription = new Prescription
       {
      ElderId = elderId,
  ElderName = request.ElderName ?? "Unknown",
 Base64Image = request.Base64Image,
         Analysis = scanResult.Data!,
       Status = "PENDING",
    UploadedAt = DateTime.UtcNow
  };

// Get assigned caretaker for this elder
var caretakerId = await _prescriptionService.GetAssignedCaretakerIdAsync(elderId);
if (!string.IsNullOrEmpty(caretakerId))
{
    prescription.CaretakerId = caretakerId;
    _logger.LogInformation($"👨‍⚕️ Assigned to Caretaker: {caretakerId}");
}
else
{
    _logger.LogWarning("⚠️ No caretaker assigned to this elder");
}

_logger.LogInformation($"📝 Prescription created with ElderId: {prescription.ElderId}");

         // Save to database
            var result = await _prescriptionService.CreatePrescriptionAsync(prescription);

          if (!result.Success)
     {
   _logger.LogError($"❌ Save failed: {result.Message}");
     return BadRequest(result);
    }

  _logger.LogInformation($"✅ Prescription saved with ID: {prescription.Id}");

       return Ok(new
    {
   success = true,
message = $"Prescription scanned using {request.ScanMethod}",
scanMethod = request.ScanMethod,
    prescriptionId = prescription.Id,
          elderId = elderId,
          caretakerId = prescription.CaretakerId,
     analysis = scanResult.Data
       });
    }

        // POST: api/Prescription/approve/{id}
        [HttpPost("approve/{id}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> ApprovePrescription(string id, [FromBody] ApprovalDto approval)
        {
            var result = await _prescriptionService.ApprovePrescriptionAsync(id, approval.IsApproved);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get pending prescriptions for caretaker
      /// </summary>
 [HttpGet("pending-for-approval")]
        [Authorize(Roles = "CARETAKER")]
      public async Task<IActionResult> GetPendingForApproval()
        {
       var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
                return Unauthorized(new { message = "Invalid token" });

            // تمام pending prescriptions نکالیں
    var result = await _prescriptionService.GetPendingPrescriptionsAsync(caretakerId);

            return Ok(result);
 }

     /// <summary>
    /// Approve or Reject prescription
        /// </summary>
        [HttpPost("{id}/review")]
        [Authorize(Roles = "CARETAKER")]
   public async Task<IActionResult> ReviewPrescription(string id, [FromBody] ReviewPrescriptionDto review)
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      if (string.IsNullOrEmpty(caretakerId))
 return Unauthorized(new { message = "Invalid token" });

 // Prescription update کریں
 var result = await _prescriptionService.UpdatePrescriptionStatusAsync(
         id,
       review.Status, // "APPROVED" یا "REJECTED"
                review.Notes
            );

       if (!result.Success)
   return BadRequest(result);

     return Ok(new
  {
            success = true,
          message = $"Prescription {review.Status.ToLower()}",
                prescriptionId = id
            });
        }

        // GET: api/Prescription/pending
        [HttpGet("pending")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetPendingPrescriptions()
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _prescriptionService.GetPendingPrescriptionsAsync(caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Prescription/my-prescriptions
        [HttpGet("my-prescriptions")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetMyPrescriptions()
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _prescriptionService.GetElderPrescriptionsAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Prescription/elder/{elderId}
        [HttpGet("elder/{elderId}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderPrescriptions(string elderId)
        {
            var result = await _prescriptionService.GetElderPrescriptionsAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Prescription/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescriptionById(string id)
        {
            var result = await _prescriptionService.GetPrescriptionByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Scan prescription using LOCAL OCR (Free - No API needed)
        /// </summary>
        [HttpPost("scan-local")]
        public async Task<IActionResult> ScanPrescriptionLocally([FromBody] ScanPrescriptionRequest request)
        {
            if (string.IsNullOrEmpty(request.Base64Image))
                return BadRequest("Image is required");

            var result = await _localScannerService.ScanPrescriptionLocallyAsync(request.Base64Image);
            return Ok(result);
        }

        /// <summary>
        /// Scan prescription using Gemini AI (Paid - Limited free quota)
        /// </summary>
        [HttpPost("scan-ai")]
        public async Task<IActionResult> ScanPrescriptionWithAI([FromBody] ScanPrescriptionRequest request)
        {
            if (string.IsNullOrEmpty(request.Base64Image))
                return BadRequest("Image is required");

            // Your existing Gemini AI logic here
            // This endpoint is kept for backward compatibility
            return await Task.FromResult(Ok(new { message = "Use /scan-local for free OCR scanning" }));
        }

        /// <summary>
        /// Get prescription details with image (for Caretaker)
        /// </summary>
      [HttpGet("{id}/view")]
        [Authorize(Roles = "CARETAKER,ELDER")]
      public async Task<IActionResult> GetPrescriptionWithImage(string id)
        {
       var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
     return Unauthorized(new { message = "Invalid token" });

            var result = await _prescriptionService.GetPrescriptionByIdAsync(id);

        if (!result.Success)
          return NotFound(result);

     var prescription = result.Data!;

          // Authorization check
     if (userRole == "ELDER" && prescription.ElderId != userId)
 return Forbid("You can only view your own prescriptions");

    if (userRole == "CARETAKER" && prescription.CaretakerId != userId)
         return Forbid("You can only view assigned elder's prescriptions");

      // Return with image
            return Ok(new
   {
       success = true,
          data = new
    {
       prescription.Id,
    prescription.ElderId,
        prescription.ElderName,
      prescription.CaretakerId,
         prescription.Status,
         prescription.IsApproved,
            prescription.UploadedAt,
          prescription.ApprovedAt,
            prescription.Notes,
         prescriptionImage = prescription.Base64Image, // Image یہاں ہے
   analysis = prescription.Analysis
      }
            });
        }

        /// <summary>
 /// Add medicine to prescription (Caretaker)
        /// </summary>
        [HttpPost("{id}/add-medicine")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> AddMedicine(string id, [FromBody] AddMedicineDto medicineDto)
        {
          var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

     if (string.IsNullOrEmpty(caretakerId))
                return Unauthorized(new { message = "Invalid token" });

  // Create ExtractedMedicine object
          var medicine = new ExtractedMedicine
   {
    MedicineName = medicineDto.MedicineName,
       Dosage = medicineDto.Dosage,
      Frequency = medicineDto.Frequency,
       Duration = medicineDto.Duration,
         SuggestedTimes = GenerateSuggestedTimes(medicineDto.Frequency),
           Warnings = GetMedicineWarnings(medicineDto.MedicineName)
            };

 var result = await _prescriptionService.AddMedicineAsync(id, caretakerId, medicine, $"Added by Caretaker");

       if (!result.Success)
         return BadRequest(result);

      return Ok(result);
     }

        // DELETE: api/Prescription/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "ELDER")]
public async Task<IActionResult> DeletePrescription(string id)
      {
    var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

 if (string.IsNullOrEmpty(elderId))
    return Unauthorized(new { message = "Invalid token" });

            var result = await _prescriptionService.DeletePrescriptionAsync(id, elderId);

            if (!result.Success)
      return BadRequest(result);

            return Ok(result);
        }

        // Helper methods
    private List<string> GenerateSuggestedTimes(string frequency)
 {
            if (string.IsNullOrEmpty(frequency))
return new List<string> { "08:00 AM", "08:00 PM" };

         var freq = frequency.ToLower();

       return freq.Contains("once") ? new List<string> { "09:00 AM" } :
       freq.Contains("twice") ? new List<string> { "08:00 AM", "08:00 PM" } :
                   freq.Contains("three") ? new List<string> { "08:00 AM", "02:00 PM", "08:00 PM" } :
 new List<string> { "08:00 AM", "08:00 PM" };
        }

        private List<string> GetMedicineWarnings(string medicineName)
        {
            var warnings = new List<string>();
            var name = medicineName.ToLower();

     if (name.Contains("paracetamol"))
   {
             warnings.Add("⚠️ Max 4000mg per day");
     warnings.Add("Avoid alcohol");
      }
 else if (name.Contains("aspirin"))
    warnings.Add("⚠️ Take after meals");
        else if (name.Contains("antibiotic"))
          warnings.Add("✅ Complete full course");

            return warnings.Count > 0 ? warnings : new List<string> { "📋 Follow doctor's instructions" };
        }

        // Helper method to get caretaker assignment
          private Task<ElderCaretakerAssignment> GetCaretakerAssignment(string elderId)
        {
              // This would come from your AssignmentService
              // For now, returning a default object to avoid nullability warning
            return Task.FromResult(new ElderCaretakerAssignment());
        }
    }

    // Simple approval DTO
    public class ApprovalDto
    {
  public bool IsApproved { get; set; }
    }

 public class ScanPrescriptionRequest
    {
        public string Base64Image { get; set; } = string.Empty;
  }
}