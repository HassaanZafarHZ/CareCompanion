using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: api/Dashboard/elder-stats
        [HttpGet("elder-stats")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetElderStats()
        {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(elderId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _dashboardService.GetElderDashboardStatsAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Dashboard/caretaker-stats
        [HttpGet("caretaker-stats")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetCaretakerStats()
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _dashboardService.GetCaretakerDashboardStatsAsync(caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}