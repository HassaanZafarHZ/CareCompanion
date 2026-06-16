using CareOS.Api.Models;
using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _reminderService;
        public ReminderController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReminders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var reminders = await _reminderService.GetRemindersByUserIdAsync(userId);
            return Ok(reminders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReminder([FromBody] Reminder reminder)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            reminder.UserId = userId;
            var created = await _reminderService.CreateReminderAsync(reminder);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReminder(string id, [FromBody] Reminder reminder)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            reminder.UserId = userId;
            var updated = await _reminderService.UpdateReminderAsync(id, reminder);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReminder(string id)
        {
            var deleted = await _reminderService.DeleteReminderAsync(id);
            return Ok(new { success = deleted });
        }
    }
}