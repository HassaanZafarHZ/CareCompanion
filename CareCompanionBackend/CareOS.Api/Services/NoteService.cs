using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class NoteService : INoteService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<CaretakerNote> _notes;

        public NoteService(MongoDbContext context)
        {
            _context = context;
            _notes = _context.GetCollection<CaretakerNote>("CaretakerNotes");
        }

        // CREATE NOTE
        public async Task<ApiResponse<CaretakerNote>> CreateNoteAsync(CreateNoteDto request, string caretakerId)
        {
            try
            {
                var note = new CaretakerNote
                {
                    ElderId = request.ElderId,
                    CaretakerId = caretakerId,
                    Title = request.Title,
                    Content = request.Content,
                    Category = request.Category,
                    IsPrivate = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _notes.InsertOneAsync(note);

                return ApiResponse<CaretakerNote>.SuccessResponse(note, "Note created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CaretakerNote>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // UPDATE NOTE
        public async Task<ApiResponse<CaretakerNote>> UpdateNoteAsync(string noteId, UpdateNoteDto request)
        {
            try
            {
                var update = Builders<CaretakerNote>.Update
                    .Set(n => n.Title, request.Title)
                    .Set(n => n.Content, request.Content)
                    .Set(n => n.Category, request.Category)
                    .Set(n => n.UpdatedAt, DateTime.UtcNow);

                var result = await _notes.FindOneAndUpdateAsync(
                    n => n.Id == noteId,
                    update,
                    new FindOneAndUpdateOptions<CaretakerNote> { ReturnDocument = ReturnDocument.After }
                );

                if (result == null)
                {
                    return ApiResponse<CaretakerNote>.ErrorResponse("Note not found");
                }

                return ApiResponse<CaretakerNote>.SuccessResponse(result, "Note updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CaretakerNote>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ELDER'S NOTES (only by assigned caretaker)
        public async Task<ApiResponse<List<CaretakerNote>>> GetElderNotesAsync(string elderId, string caretakerId)
        {
            try
            {
                List<CaretakerNote> notes;
                if (!string.IsNullOrEmpty(elderId))
                {
                    // Elder: get all notes for this elder
                    notes = await _notes
                        .Find(n => n.ElderId == elderId)
                        .SortByDescending(n => n.UpdatedAt)
                        .ToListAsync();
                }
                else if (!string.IsNullOrEmpty(caretakerId))
                {
                    // Caretaker: get all notes for this caretaker
                    notes = await _notes
                        .Find(n => n.CaretakerId == caretakerId)
                        .SortByDescending(n => n.UpdatedAt)
                        .ToListAsync();
                }
                else
                {
                    notes = new List<CaretakerNote>();
                }
                return ApiResponse<List<CaretakerNote>>.SuccessResponse(notes);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CaretakerNote>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET NOTE BY ID
        public async Task<ApiResponse<CaretakerNote>> GetNoteByIdAsync(string noteId)
        {
            try
            {
                var note = await _notes.Find(n => n.Id == noteId).FirstOrDefaultAsync();

                if (note == null)
                {
                    return ApiResponse<CaretakerNote>.ErrorResponse("Note not found");
                }

                return ApiResponse<CaretakerNote>.SuccessResponse(note);
            }
            catch (Exception ex)
            {
                return ApiResponse<CaretakerNote>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // DELETE NOTE
        public async Task<ApiResponse<bool>> DeleteNoteAsync(string noteId)
        {
            try
            {
                var result = await _notes.DeleteOneAsync(n => n.Id == noteId);

                if (result.DeletedCount > 0)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Note deleted successfully");
                }

                return ApiResponse<bool>.ErrorResponse("Note not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}