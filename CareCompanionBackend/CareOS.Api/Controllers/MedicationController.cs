using CareOS.Api.DTOs;
using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _medicationService;

        public MedicationController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }

        // POST: api/Medication/create
        [HttpPost("create")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> CreateMedication([FromBody] CreateMedicationDto request)
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _medicationService.CreateMedicationAsync(request, caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Medication/approve
        [HttpPost("approve")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> ApproveMedication([FromBody] ApproveMedicationDto request)
        {
            var result = await _medicationService.ApproveMedicationAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Medication/confirm-taken
        [HttpPost("confirm-taken")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> ConfirmMedicationTaken([FromBody] ConfirmMedicationDto request)
        {
            var result = await _medicationService.ConfirmMedicationTakenAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Medication/my-medications (Elder)
        [HttpGet("my-medications")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetMyMedications()
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _medicationService.GetElderMedicationsAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Medication/elder/{elderId} (Caretaker view)
        [HttpGet("elder/{elderId}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderMedications(string elderId)
        {
            var result = await _medicationService.GetElderMedicationsAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Medication/pending-approvals
        [HttpGet("pending-approvals")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _medicationService.GetPendingApprovalsAsync(caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Medication/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedicationById(string id)
        {
            var result = await _medicationService.GetMedicationByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        // DELETE: api/Medication/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> DeleteMedication(string id)
        {
            var result = await _medicationService.DeleteMedicationAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}