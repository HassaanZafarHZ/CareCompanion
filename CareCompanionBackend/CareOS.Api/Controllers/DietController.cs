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
    public class DietController : ControllerBase
    {
        private readonly IDietService _dietService;

        public DietController(IDietService dietService)
        {
            _dietService = dietService;
        }

        // POST: api/Diet/create
        [HttpPost("create")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> CreateDietSchedule([FromBody] CreateDietScheduleDto request)
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _dietService.CreateDietScheduleAsync(request, caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Diet/complete
        [HttpPost("complete")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> CompleteDiet([FromBody] CompleteDietDto request)
        {
            var result = await _dietService.CompleteDietAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Diet/today
        [HttpGet("today")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetTodayDiet()
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _dietService.GetTodayDietAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Diet/elder/{elderId}/today (Caretaker view)
        [HttpGet("elder/{elderId}/today")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderTodayDiet(string elderId)
        {
            var result = await _dietService.GetTodayDietAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Diet/elder/{elderId}/date?date=2025-12-31
        [HttpGet("elder/{elderId}/date")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderDietByDate(string elderId, [FromQuery] DateTime date)
        {
            var result = await _dietService.GetElderDietScheduleAsync(elderId, date);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}