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
    public class EmergencyController : ControllerBase
    {
        private readonly IEmergencyService _emergencyService;

        public EmergencyController(IEmergencyService emergencyService)
        {
            _emergencyService = emergencyService;
        }

        // POST: api/Emergency/trigger
        [HttpPost("trigger")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> TriggerEmergency([FromBody] TriggerEmergencyDto request)
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ensure elder is triggering for themselves
            if (request.ElderId != elderId)
            {
                return Unauthorized(new { message = "You can only trigger emergency for yourself" });
            }

            var result = await _emergencyService.TriggerEmergencyAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Emergency/acknowledge
        [HttpPost("acknowledge")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> AcknowledgeEmergency([FromBody] AcknowledgeEmergencyDto request)
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (request.CaretakerId != caretakerId)
            {
                return Unauthorized(new { message = "Invalid caretaker" });
            }

            var result = await _emergencyService.AcknowledgeEmergencyAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Emergency/my-alerts (Elder)
        [HttpGet("my-alerts")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetMyAlerts()
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _emergencyService.GetElderAlertsAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Emergency/caretaker-alerts (Caretaker)
        [HttpGet("caretaker-alerts")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetCaretakerAlerts()
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _emergencyService.GetCaretakerAlertsAsync(caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Emergency/pending
        [HttpGet("pending")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetPendingAlerts()
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _emergencyService.GetPendingAlertsAsync(caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Emergency/resolve/{id}
        [HttpPost("resolve/{id}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> ResolveAlert(string id)
        {
            var result = await _emergencyService.ResolveAlertAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}