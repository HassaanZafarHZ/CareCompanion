
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
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // GET: api/Task
        [HttpGet]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetCaretakerTasks()
        {
            var caretakerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            var result = await _taskService.GetTasksByCaretakerIdAsync(caretakerId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // POST: api/Task/create
        [HttpPost("create")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto request)
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _taskService.CreateTaskAsync(request, caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Task/complete/{id}
        [HttpPost("complete/{id}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> CompleteTask(string id)
        {
            var result = await _taskService.CompleteTaskAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Task/elder/{elderId}/today
        [HttpGet("elder/{elderId}/today")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetTodayTasks(string elderId)
        {
            var result = await _taskService.GetTodayTasksAsync(elderId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Task/elder/{elderId}?date=2025-12-31
        [HttpGet("elder/{elderId}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> GetElderTasks(string elderId, [FromQuery] DateTime? date = null)
        {
            var result = await _taskService.GetElderTasksAsync(elderId, date);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // DELETE: api/Task/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "CARETAKER")]
        public async Task<IActionResult> DeleteTask(string id)
        {
            var result = await _taskService.DeleteTaskAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}