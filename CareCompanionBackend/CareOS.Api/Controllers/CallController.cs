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
    public class CallController : ControllerBase
    {
        private readonly ICallService _callService;

        public CallController(ICallService callService)
        {
            _callService = callService;
        }

        // POST: api/Call/initiate
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateCall([FromBody] InitiateCallDto request)
        {
            var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (request.CallerId != callerId)
            {
                return Unauthorized(new { message = "You can only initiate calls from your own account" });
            }

            var result = await _callService.InitiateCallAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Call/accept/{callId}
        [HttpPost("accept/{callId}")]
        public async Task<IActionResult> AcceptCall(string callId)
        {
            var result = await _callService.AcceptCallAsync(callId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Call/decline/{callId}
        [HttpPost("decline/{callId}")]
        public async Task<IActionResult> DeclineCall(string callId)
        {
            var result = await _callService.DeclineCallAsync(callId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Call/end
        [HttpPost("end")]
        public async Task<IActionResult> EndCall([FromBody] EndCallDto request)
        {
            var result = await _callService.EndCallAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Call/history
        [HttpGet("history")]
        public async Task<IActionResult> GetCallHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _callService.GetCallHistoryAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET: api/Call/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCall()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _callService.GetActiveCallAsync(userId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}