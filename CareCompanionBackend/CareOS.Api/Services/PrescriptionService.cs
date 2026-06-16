using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<Prescription> _prescriptions;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<ElderCaretakerAssignment> _assignments;
        private readonly IAiService _aiService;

        public PrescriptionService(MongoDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
            _prescriptions = _context.GetCollection<Prescription>("Prescriptions");
            _users = _context.GetCollection<User>("Users");
            _assignments = _context.GetCollection<ElderCaretakerAssignment>("Assignments");
        }

        // UPLOAD & ANALYZE PRESCRIPTION
        public async Task<ApiResponse<Prescription>> UploadPrescriptionAsync(UploadPrescriptionDto request)
        {
            try
            {
                // Get elder details
                var elder = await _users.Find(u => u.Id == request.ElderId).FirstOrDefaultAsync();
                if (elder == null)
                {
                    return ApiResponse<Prescription>.ErrorResponse("Elder not found");
                }

                // Get assigned caretaker
                var assignment = await _assignments
                    .Find(a => a.ElderId == request.ElderId && a.IsActive)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return ApiResponse<Prescription>.ErrorResponse("No caretaker assigned");
                }

                // AI ANALYSIS - Prescription scan
                var analysisResult = await _aiService.AnalyzePrescriptionAsync(request.Base64Image);

                if (!analysisResult.Success)
                {
                    return ApiResponse<Prescription>.ErrorResponse("Failed to analyze prescription");
                }

                // Create prescription record
                var prescription = new Prescription
                {
                    ElderId = request.ElderId,
                    ElderName = elder.FullName,
                    CaretakerId = assignment.CaretakerId,
                    Base64Image = request.Base64Image,
                    PrescriptionImageUrl = $"data:image/jpeg;base64,{request.Base64Image.Substring(0, 50)}...", // Mock URL
                    Analysis = analysisResult.Data!,
                    IsApproved = false,
                    Status = "PENDING",
                    Notes = request.Notes,
                    UploadedAt = DateTime.UtcNow
                };

                await _prescriptions.InsertOneAsync(prescription);

                return ApiResponse<Prescription>.SuccessResponse(
                    prescription,
                    "Prescription uploaded and analyzed. Waiting for caretaker approval."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<Prescription>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // APPROVE/REJECT PRESCRIPTION
        public async Task<ApiResponse<Prescription>> ApprovePrescriptionAsync(string prescriptionId, bool isApproved)
        {
            try
            {
                var prescription = await _prescriptions.Find(p => p.Id == prescriptionId).FirstOrDefaultAsync();

                if (prescription == null)
                {
                    return ApiResponse<Prescription>.ErrorResponse("Prescription not found");
                }

                prescription.IsApproved = isApproved;
                prescription.Status = isApproved ? "APPROVED" : "REJECTED";
                prescription.ApprovedAt = DateTime.UtcNow;

                var update = Builders<Prescription>.Update
                    .Set(p => p.IsApproved, isApproved)
                    .Set(p => p.Status, prescription.Status)
                    .Set(p => p.ApprovedAt, prescription.ApprovedAt);

                await _prescriptions.UpdateOneAsync(p => p.Id == prescriptionId, update);

                // If APPROVED, create medication schedules automatically
                if (isApproved && prescription.Analysis.Medicines.Any())
                {
                    // TODO: Auto-create medication schedules
                    // foreach (var med in prescription.Analysis.Medicines)
                    // {
                    //     await _medicationService.CreateMedicationAsync(...)
                    // }
                }

                return ApiResponse<Prescription>.SuccessResponse(
                    prescription,
                    isApproved ? "Prescription approved" : "Prescription rejected"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<Prescription>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET PENDING PRESCRIPTIONS (Caretaker)
        public async Task<ApiResponse<List<Prescription>>> GetPendingPrescriptionsAsync(string caretakerId)
        {
            try
            {
                var prescriptions = await _prescriptions
                    .Find(p => p.CaretakerId == caretakerId && p.Status == "PENDING")
                    .SortByDescending(p => p.UploadedAt)
                    .ToListAsync();

                return ApiResponse<List<Prescription>>.SuccessResponse(
                    prescriptions,
                    $"Found {prescriptions.Count} pending prescriptions"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Prescription>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ELDER'S PRESCRIPTIONS
        public async Task<ApiResponse<List<Prescription>>> GetElderPrescriptionsAsync(string elderId)
        {
            try
            {
                var prescriptions = await _prescriptions
                    .Find(p => p.ElderId == elderId)
                    .SortByDescending(p => p.UploadedAt)
                    .ToListAsync();

                return ApiResponse<List<Prescription>>.SuccessResponse(prescriptions);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Prescription>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET PRESCRIPTION BY ID
        public async Task<ApiResponse<Prescription>> GetPrescriptionByIdAsync(string prescriptionId)
        {
            try
            {
                var prescription = await _prescriptions.Find(p => p.Id == prescriptionId).FirstOrDefaultAsync();

                if (prescription == null)
                {
                    return ApiResponse<Prescription>.ErrorResponse("Prescription not found");
                }

                return ApiResponse<Prescription>.SuccessResponse(prescription);
            }
            catch (Exception ex)
            {
                return ApiResponse<Prescription>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // CREATE PRESCRIPTION (Local OCR scan کے بعد)
        public async Task<ApiResponse<Prescription>> CreatePrescriptionAsync(Prescription prescription)
        {
            try
            {
                if (string.IsNullOrEmpty(prescription.ElderId))
                    return ApiResponse<Prescription>.ErrorResponse("Elder ID is required");

                await _prescriptions.InsertOneAsync(prescription);

                return ApiResponse<Prescription>.SuccessResponse(
                    prescription,
                    "Prescription created successfully. Awaiting caretaker approval."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<Prescription>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // UPDATE PRESCRIPTION STATUS (Caretaker کے لیے)
        public async Task<ApiResponse<Prescription>> UpdatePrescriptionStatusAsync(string prescriptionId, string status, string? notes)
        {
            try
            {
                var prescription = await _prescriptions.Find(p => p.Id == prescriptionId).FirstOrDefaultAsync();

                if (prescription == null)
                    return ApiResponse<Prescription>.ErrorResponse("Prescription not found");

                var validStatuses = new[] { "PENDING", "APPROVED", "REJECTED", "MODIFIED" };
                if (!validStatuses.Contains(status.ToUpper()))
                    return ApiResponse<Prescription>.ErrorResponse("Invalid status");

                var update = Builders<Prescription>.Update
                    .Set(p => p.Status, status.ToUpper())
                    .Set(p => p.IsApproved, status.ToUpper() == "APPROVED")
                    .Set(p => p.ApprovedAt, DateTime.UtcNow)
                    .Set(p => p.Notes, notes);

                await _prescriptions.UpdateOneAsync(p => p.Id == prescriptionId, update);

                prescription.Status = status.ToUpper();
                prescription.IsApproved = status.ToUpper() == "APPROVED";
                prescription.ApprovedAt = DateTime.UtcNow;
                prescription.Notes = notes;

                return ApiResponse<Prescription>.SuccessResponse(
                    prescription,
                    $"Prescription status updated to {status.ToUpper()}"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<Prescription>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // ADD MEDICINE BY CARETAKER
        public async Task<ApiResponse<Prescription>> AddMedicineAsync(string prescriptionId, string caretakerId, ExtractedMedicine medicine, string? editNotes)
        {
            try
            {
                var prescription = await _prescriptions.Find(p => p.Id == prescriptionId).FirstOrDefaultAsync();

                if (prescription == null)
                    return ApiResponse<Prescription>.ErrorResponse("Prescription not found");

                if (prescription.CaretakerId != caretakerId)
                    return ApiResponse<Prescription>.ErrorResponse("Unauthorized to edit this prescription");

                // Existing medicines میں add کریں
                prescription.Analysis.Medicines.Add(medicine);

                // Track اسے AddedMedicines میں
                if (prescription.AddedMedicines == null)
                    prescription.AddedMedicines = new List<ExtractedMedicine>();

                prescription.AddedMedicines.Add(medicine);

                // Update database
                var update = Builders<Prescription>.Update
                    .Set(p => p.Analysis, prescription.Analysis)
                    .Set(p => p.AddedMedicines, prescription.AddedMedicines)
                    .Set(p => p.Status, "MODIFIED")
                    .Set(p => p.EditedBy, caretakerId)
                    .Set(p => p.EditNotes, editNotes);

                await _prescriptions.UpdateOneAsync(p => p.Id == prescriptionId, update);

                return ApiResponse<Prescription>.SuccessResponse(
                    prescription,
                    $"Medicine '{medicine.MedicineName}' added successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<Prescription>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // EDIT MEDICINE BY CARETAKER
        public async Task<ApiResponse<Prescription>> EditMedicineAsync(string prescriptionId, string caretakerId, int medicineIndex, EditMedicineDto editData)
        {
            try
            {
                var prescription = await _prescriptions.Find(p => p.Id == prescriptionId).FirstOrDefaultAsync();

                if (prescription == null)
                    return ApiResponse<Prescription>.ErrorResponse("Prescription not found");

                if (prescription.CaretakerId != caretakerId)
                    return ApiResponse<Prescription>.ErrorResponse("Unauthorized to edit this prescription");

                if (medicineIndex < 0 || medicineIndex >= prescription.Analysis.Medicines.Count)
                    return ApiResponse<Prescription>.ErrorResponse("Invalid medicine index");

                // Update medicine
                var medicine = prescription.Analysis.Medicines[medicineIndex];
                if (!string.IsNullOrEmpty(editData.MedicineName))
                    medicine.MedicineName = editData.MedicineName;
                if (!string.IsNullOrEmpty(editData.Dosage))
                    medicine.Dosage = editData.Dosage;
                if (!string.IsNullOrEmpty(editData.Frequency))
                    medicine.Frequency = editData.Frequency;
                if (!string.IsNullOrEmpty(editData.Duration))
                    medicine.Duration = editData.Duration;

                // Update database
                var update = Builders<Prescription>.Update
                    .Set(p => p.Analysis, prescription.Analysis)
                    .Set(p => p.Status, "MODIFIED")
                    .Set(p => p.EditedBy, caretakerId)
                    .Set(p => p.EditNotes, editData.EditNotes);

                await _prescriptions.UpdateOneAsync(p => p.Id == prescriptionId, update);

                return ApiResponse<Prescription>.SuccessResponse(
                    prescription,
                    $"Medicine updated successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<Prescription>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // DELETE PRESCRIPTION (Elder only - sirf apni prescription delete kar sakta hai)
        public async Task<ApiResponse<bool>> DeletePrescriptionAsync(string prescriptionId, string elderId)
        {
            try
            {
                var prescription = await _prescriptions.Find(p => p.Id == prescriptionId).FirstOrDefaultAsync();

                if (prescription == null)
                    return ApiResponse<bool>.ErrorResponse("Prescription not found");

                // Only the owner (elder) can delete their prescription
                if (prescription.ElderId != elderId)
                    return ApiResponse<bool>.ErrorResponse("You can only delete your own prescriptions");

                // Can only delete PENDING or REJECTED prescriptions (not approved ones)
                if (prescription.Status == "APPROVED")
                    return ApiResponse<bool>.ErrorResponse("Cannot delete approved prescriptions");

                await _prescriptions.DeleteOneAsync(p => p.Id == prescriptionId);

                return ApiResponse<bool>.SuccessResponse(true, "Prescription deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ASSIGNED CARETAKER ID FOR ELDER
        public async Task<string?> GetAssignedCaretakerIdAsync(string elderId)
        {
            try
            {
                var assignment = await _assignments
                    .Find(a => a.ElderId == elderId && a.Status == "APPROVED" && a.IsActive)
                    .FirstOrDefaultAsync();

                return assignment?.CaretakerId;
            }
            catch
            {
                return null;
            }
        }
    }
}