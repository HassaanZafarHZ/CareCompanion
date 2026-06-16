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
    public class CheckInController : ControllerBase
    {
        private readonly IDailyCheckInService _checkInService;

        public CheckInController(IDailyCheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        // POST: api/CheckIn/create
        [HttpPost("create")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> CreateCheckIn([FromBody] CreateCheckInDto request)
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ensure elder is checking in for themselves
            if (request.ElderId != elderId)
            {
                return Unauthorized(new { message = "You can only check in for yourself" });
            }

            var result = await _checkInService.CreateCheckInAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/CheckIn/my-history?days=7
        [HttpGet("my-history")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetMyHistory([FromQuery] int days = 7)
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _checkInService.GetElderCheckInsAsync(elderId, days);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/CheckIn/elder/{elderId}/history?days=7 (Caretaker view)
        [HttpGet("elder/{elderId}/history")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderHistory(string elderId, [FromQuery] int days = 7)
        {
            var result = await _checkInService.GetElderCheckInsAsync(elderId, days);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/CheckIn/today
        [HttpGet("today")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetTodayCheckIn()
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _checkInService.GetTodayCheckInAsync(elderId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}