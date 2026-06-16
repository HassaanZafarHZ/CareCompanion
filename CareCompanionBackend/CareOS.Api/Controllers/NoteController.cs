using CareOS.Api.DTOs;
using CareOS.Api.Services;
using CareOS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "CARETAKER")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        // GET: api/Note
        [HttpGet]
        [Authorize(Roles = "CARETAKER,ELDER")]
        public async Task<IActionResult> GetMyNotes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token" });

            // For now, show all notes where ElderId == userId (for elders) or CaretakerId == userId (for caretakers)

            var isElder = User.IsInRole("ELDER");
            ApiResponse<List<CaretakerNote>> result;
            if (isElder)
            {
                // Elder: get all notes for this elder
                result = await _noteService.GetElderNotesAsync(userId, "");
            }
            else
            {
                // Caretaker: get all notes for this caretaker
                result = await _noteService.GetElderNotesAsync("", userId);
            }

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // POST: api/Note/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteDto request)
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _noteService.CreateNoteAsync(request, caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // PUT: api/Note/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(string id, [FromBody] UpdateNoteDto request)
        {
            var result = await _noteService.UpdateNoteAsync(id, request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Note/elder/{elderId}
        [HttpGet("elder/{elderId}")]
        public async Task<IActionResult> GetElderNotes(string elderId)
        {
            var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _noteService.GetElderNotesAsync(elderId, caretakerId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Note/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(string id)
        {
            var result = await _noteService.GetNoteByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        // DELETE: api/Note/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            var result = await _noteService.DeleteNoteAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}