using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface INoteService
    {
        Task<ApiResponse<CaretakerNote>> CreateNoteAsync(CreateNoteDto request, string caretakerId);
        Task<ApiResponse<CaretakerNote>> UpdateNoteAsync(string noteId, UpdateNoteDto request);
        Task<ApiResponse<List<CaretakerNote>>> GetElderNotesAsync(string elderId, string caretakerId);
        Task<ApiResponse<CaretakerNote>> GetNoteByIdAsync(string noteId);
        Task<ApiResponse<bool>> DeleteNoteAsync(string noteId);
    }
}