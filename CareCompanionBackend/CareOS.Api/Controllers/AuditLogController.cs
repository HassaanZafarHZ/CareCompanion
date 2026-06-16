using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: api/AuditLog/my-logs?days=30
        [HttpGet("my-logs")]
        public async Task<IActionResult> GetMyLogs([FromQuery] int days = 30)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _auditLogService.GetUserLogsAsync(userId, days);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/AuditLog/all?days=7 (Caretaker only)
        [HttpGet("all")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetAllLogs([FromQuery] int days = 7)
        {
            var result = await _auditLogService.GetAllLogsAsync(days);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/AuditLog/action/{action}?days=7
        [HttpGet("action/{action}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetLogsByAction(string action, [FromQuery] int days = 7)
        {
            var result = await _auditLogService.GetLogsByActionAsync(action, days);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/AuditLog/entity/{entityType}/{entityId}
        [HttpGet("entity/{entityType}/{entityId}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetLogsByEntity(string entityType, string entityId)
        {
            var result = await _auditLogService.GetLogsByEntityAsync(entityType, entityId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}