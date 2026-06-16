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
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        // POST: api/Activity/create
        [HttpPost("create")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> CreateActivitySchedule([FromBody] CreateActivityScheduleDto request)
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _activityService.CreateActivityScheduleAsync(request, caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Activity/complete
        [HttpPost("complete")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> CompleteActivity([FromBody] CompleteActivityDto request)
        {
            var result = await _activityService.CompleteActivityAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Activity/today
        [HttpGet("today")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetTodayActivities()
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _activityService.GetTodayActivitiesAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Activity/elder/{elderId}/today (Caretaker view)
        [HttpGet("elder/{elderId}/today")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderTodayActivities(string elderId)
        {
            var result = await _activityService.GetTodayActivitiesAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Activity/elder/{elderId}/date?date=2025-12-31
        [HttpGet("elder/{elderId}/date")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderActivitiesByDate(string elderId, [FromQuery] DateTime date)
        {
            var result = await _activityService.GetElderActivitiesAsync(elderId, date);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}