using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface IAssignmentService
    {
        Task<ApiResponse<List<AvailableCaretakerDto>>> GetAvailableCaretakersAsync();
        Task<ApiResponse<ElderCaretakerAssignment>> AssignCaretakerAsync(AssignCaretakerDto request);
        Task<ApiResponse<ElderCaretakerAssignment>> GetAssignmentByElderIdAsync(string elderId);
        Task<ApiResponse<List<ElderCaretakerAssignment>>> GetAssignmentsByCaretakerIdAsync(string caretakerId);
        Task<ApiResponse<bool>> RemoveAssignmentAsync(string assignmentId);
        Task<ApiResponse<ElderCaretakerAssignment>> RespondToRequestAsync(string assignmentId, string caretakerId, bool approve);
        Task<ApiResponse<List<ElderCaretakerAssignment>>> GetPendingRequestsAsync(string caretakerId);
        Task<ApiResponse<List<ElderCaretakerAssignment>>> GetMySentRequestsAsync(string elderId);
    }
}