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
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(IAssignmentService assignmentService)
        {
         _assignmentService = assignmentService;
    }

        // GET: api/Assignment/available-caretakers
 [HttpGet("available-caretakers")]
 public async Task<IActionResult> GetAvailableCaretakers()
 {
            var result = await _assignmentService.GetAvailableCaretakersAsync();
          return result.Success ? Ok(result) : BadRequest(result);
        }

        // POST: api/Assignment/request (Elder sends request to a caretaker)
        [HttpPost("request")]
        [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> SendRequest([FromBody] AssignCaretakerDto request)
   {
  var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

  if (request.ElderId != currentUserId)
            {
     return Unauthorized(new { message = "You can only send request for yourself" });
            }

        var result = await _assignmentService.AssignCaretakerAsync(request);
  return result.Success ? Ok(result) : BadRequest(result);
        }

        // Backward compatibility
        [HttpPost("assign")]
    [Authorize(Roles = "ELDER")]
  public async Task<IActionResult> AssignCaretaker([FromBody] AssignCaretakerDto request)
      {
    return await SendRequest(request);
        }

        // GET: api/Assignment/my-sent-requests (Elder sees all sent requests)
    [HttpGet("my-sent-requests")]
      [Authorize(Roles = "ELDER")]
        public async Task<IActionResult> GetMySentRequests()
        {
          // TEMP LOGGING: Print all claims for debugging
          Console.WriteLine("--- JWT Claims ---");
          foreach (var claim in User.Claims)
          {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
          }
          Console.WriteLine($"IsInRole(ELDER): {User.IsInRole("ELDER")}");
          Console.WriteLine($"NameIdentifier: {User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}");

          var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
          if (string.IsNullOrEmpty(elderId))
            return Unauthorized(new { message = "Invalid token" });

          var result = await _assignmentService.GetMySentRequestsAsync(elderId);
          return Ok(result);
        }

        // GET: api/Assignment/pending-requests (Caretaker sees pending requests)
  [HttpGet("pending-requests")]
  [Authorize(Roles = "CARETAKER")]
    public async Task<IActionResult> GetPendingRequests()
        {
    var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

         if (string.IsNullOrEmpty(caretakerId))
            return Unauthorized(new { message = "Invalid token" });

    var result = await _assignmentService.GetPendingRequestsAsync(caretakerId);
  return Ok(result);
        }

        // POST: api/Assignment/{id}/approve
      [HttpPost("{id}/approve")]
        [Authorize(Roles = "CARETAKER")]
   public async Task<IActionResult> ApproveRequest(string id)
   {
     var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(caretakerId))
        return Unauthorized(new { message = "Invalid token" });

 var result = await _assignmentService.RespondToRequestAsync(id, caretakerId, true);
       return result.Success ? Ok(result) : BadRequest(result);
   }

 // POST: api/Assignment/{id}/reject
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "CARETAKER")]
   public async Task<IActionResult> RejectRequest(string id)
        {
  var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

       if (string.IsNullOrEmpty(caretakerId))
     return Unauthorized(new { message = "Invalid token" });

       var result = await _assignmentService.RespondToRequestAsync(id, caretakerId, false);
            return result.Success ? Ok(result) : BadRequest(result);
        }

     // GET: api/Assignment/my-assignment (Elder - only APPROVED)
        [HttpGet("my-assignment")]
  [Authorize(Roles = "ELDER")]
      public async Task<IActionResult> GetMyAssignment()
      {
            var elderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

 if (string.IsNullOrEmpty(elderId))
                return Unauthorized(new { message = "Invalid token" });

     var result = await _assignmentService.GetAssignmentByElderIdAsync(elderId);
     if (!result.Success || result.Data == null)
     {
       return NotFound(result);
     }
     // Map to expected frontend structure
     var assignment = result.Data;
     var response = new {
       success = true,
       data = new {
         caretaker = new {
           id = assignment.CaretakerId,
           name = assignment.CaretakerName
         }
       }
     };
     return Ok(response);
        }

        // GET: api/Assignment/my-elders (Caretaker - only APPROVED)
        [HttpGet("my-elders")]
        [Authorize(Roles = "CARETAKER")]
     public async Task<IActionResult> GetMyElders()
        {
   var caretakerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      if (string.IsNullOrEmpty(caretakerId))
   return Unauthorized(new { message = "Invalid token" });

          var result = await _assignmentService.GetAssignmentsByCaretakerIdAsync(caretakerId);
return Ok(result);
    }

        // DELETE: api/Assignment/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "ELDER,CARETAKER")]
      public async Task<IActionResult> RemoveAssignment(string id)
        {
  var result = await _assignmentService.RemoveAssignmentAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}