using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HealthController : ControllerBase
    {
        private readonly IHealthService _healthService;

        public HealthController(IHealthService healthService)
        {
            _healthService = healthService;
        }

        // POST: api/Health/bp/record
        [HttpPost("bp/record")]
        [Authorize(Roles = "ELDER,CARETAKER")]
        public async Task<IActionResult> RecordBloodPressure([FromBody] RecordBPDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            // Elder records for themselves, Caretaker can record for elder
            var result = await _healthService.RecordBloodPressureAsync(userId, request.Systolic, request.Diastolic);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Health/bp/history?days=30
        [HttpGet("bp/history")]
        public async Task<IActionResult> GetBPHistory([FromQuery] int days = 30)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _healthService.GetBPHistoryAsync(userId, days);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Health/bp/latest
        [HttpGet("bp/latest")]
        public async Task<IActionResult> GetLatestBP()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _healthService.GetLatestBPAsync(userId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        // GET: api/Health/bp/elder/{elderId}/latest (Caretaker view)
        [HttpGet("bp/elder/{elderId}/latest")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderLatestBP(string elderId)
        {
            var result = await _healthService.GetLatestBPAsync(elderId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        // GET: api/Health/bp/elder/{elderId}/history?days=30
        [HttpGet("bp/elder/{elderId}/history")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderBPHistory(string elderId, [FromQuery] int days = 30)
        {
            var result = await _healthService.GetBPHistoryAsync(elderId, days);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}