using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<ElderCaretakerAssignment> _assignments;

        public AssignmentService(MongoDbContext context)
        {
            _context = context;
            _users = _context.GetCollection<User>("Users");
            _assignments = _context.GetCollection<ElderCaretakerAssignment>("Assignments");
        }

        // GET AVAILABLE CARETAKERS
        public async Task<ApiResponse<List<AvailableCaretakerDto>>> GetAvailableCaretakersAsync()
        {
            try
            {
                var caretakers = await _users.Find(u => u.Role == "CARETAKER" && u.IsActive).ToListAsync();
                var availableList = new List<AvailableCaretakerDto>();

                foreach (var caretaker in caretakers)
                {
                    // Count sirf APPROVED elders
                    var assignedCount = await _assignments.CountDocumentsAsync(
                        a => a.CaretakerId == caretaker.Id && a.IsActive && a.Status == "APPROVED"
                    );

                    availableList.Add(new AvailableCaretakerDto
                    {
                        Id = caretaker.Id,
                        FullName = caretaker.FullName,
                        Email = caretaker.Email,
                        PhoneNumber = caretaker.PhoneNumber,
                        ProfilePicture = caretaker.ProfilePicture,
                        AssignedEldersCount = (int)assignedCount,
                        IsAvailable = assignedCount < 3
                    });
                }

                return ApiResponse<List<AvailableCaretakerDto>>.SuccessResponse(availableList);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AvailableCaretakerDto>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // SEND REQUEST TO CARETAKER (Elder can send to MULTIPLE caretakers)
        public async Task<ApiResponse<ElderCaretakerAssignment>> AssignCaretakerAsync(AssignCaretakerDto request)
        {
            try
            {
                // CHECK: Elder already has an APPROVED caretaker?
                var approvedAssignment = await _assignments.Find(
                    a => a.ElderId == request.ElderId && a.Status == "APPROVED" && a.IsActive
                ).FirstOrDefaultAsync();

                if (approvedAssignment != null)
                {
                    return ApiResponse<ElderCaretakerAssignment>.ErrorResponse(
                        "You already have an assigned caretaker"
                    );
                }

                // CHECK: Already sent request to THIS caretaker?
                var existingRequest = await _assignments.Find(
                    a => a.ElderId == request.ElderId &&
                         a.CaretakerId == request.CaretakerId &&
                         a.Status == "PENDING"
                ).FirstOrDefaultAsync();

                if (existingRequest != null)
                {
                    return ApiResponse<ElderCaretakerAssignment>.ErrorResponse(
                        "You already sent a request to this caretaker"
                    );
                }

                // CHECK: Caretaker has 3 elders already?
                var caretakerCount = await _assignments.CountDocumentsAsync(
                    a => a.CaretakerId == request.CaretakerId && a.Status == "APPROVED" && a.IsActive
                );

                if (caretakerCount >= 3)
                {
                    return ApiResponse<ElderCaretakerAssignment>.ErrorResponse(
                        "This caretaker already has maximum 3 elders"
                    );
                }

                // Fetch details
                var elder = await _users.Find(u => u.Id == request.ElderId).FirstOrDefaultAsync();
                var caretaker = await _users.Find(u => u.Id == request.CaretakerId).FirstOrDefaultAsync();

                if (elder == null || caretaker == null)
                {
                    return ApiResponse<ElderCaretakerAssignment>.ErrorResponse("Invalid Elder or Caretaker ID");
                }

                // Create PENDING request
                var assignment = new ElderCaretakerAssignment
                {
                    ElderId = request.ElderId,
                    CaretakerId = request.CaretakerId,
                    ElderName = elder.FullName,
                    CaretakerName = caretaker.FullName,
                    ElderPhone = elder.PhoneNumber,
                    ElderEmail = elder.Email,
                    Notes = request.Notes,
                    AssignedAt = DateTime.UtcNow,
                    Status = "PENDING",
                    IsActive = true
                };

                await _assignments.InsertOneAsync(assignment);

                return ApiResponse<ElderCaretakerAssignment>.SuccessResponse(
                    assignment,
                    "Request sent! Waiting for caretaker approval."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<ElderCaretakerAssignment>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // APPROVE/REJECT REQUEST (Caretaker)
        public async Task<ApiResponse<ElderCaretakerAssignment>> RespondToRequestAsync(string assignmentId, string caretakerId, bool approve)
        {
            try
            {
                var assignment = await _assignments.Find(
                    a => a.Id == assignmentId && a.CaretakerId == caretakerId && a.Status == "PENDING"
                ).FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return ApiResponse<ElderCaretakerAssignment>.ErrorResponse("Request not found or already processed");
                }

                // If APPROVING - extra checks
                if (approve)
                {
                    // Check: Elder already has approved caretaker?
                    var elderAlreadyAssigned = await _assignments.Find(
                        a => a.ElderId == assignment.ElderId && a.Status == "APPROVED" && a.IsActive
                    ).FirstOrDefaultAsync();

                    if (elderAlreadyAssigned != null)
                    {
                        // Auto-reject this request as elder already has caretaker
                        var rejectUpdate = Builders<ElderCaretakerAssignment>.Update
                            .Set(a => a.Status, "AUTO_REJECTED")
                            .Set(a => a.Notes, "Elder already has an assigned caretaker")
                            .Set(a => a.IsActive, false);
                        await _assignments.UpdateOneAsync(a => a.Id == assignmentId, rejectUpdate);

                        return ApiResponse<ElderCaretakerAssignment>.ErrorResponse(
                            "This elder already has an assigned caretaker"
                        );
                    }

                    // Check: Caretaker has 3 elders?
                    var caretakerCount = await _assignments.CountDocumentsAsync(
                        a => a.CaretakerId == caretakerId && a.Status == "APPROVED" && a.IsActive
                    );

                    if (caretakerCount >= 3)
                    {
                        return ApiResponse<ElderCaretakerAssignment>.ErrorResponse(
                            "You already have maximum 3 elders"
                        );
                    }

                    // APPROVE this request
                    var approveUpdate = Builders<ElderCaretakerAssignment>.Update
                        .Set(a => a.Status, "APPROVED")
                        .Set(a => a.ApprovedAt, DateTime.UtcNow)
                        .Set(a => a.IsActive, true);
                    await _assignments.UpdateOneAsync(a => a.Id == assignmentId, approveUpdate);

                    // AUTO-CANCEL all other PENDING requests from this elder
                    var cancelUpdate = Builders<ElderCaretakerAssignment>.Update
                        .Set(a => a.Status, "AUTO_CANCELLED")
                        .Set(a => a.Notes, "Another caretaker accepted the request")
                        .Set(a => a.IsActive, false);

                    await _assignments.UpdateManyAsync(
                        a => a.ElderId == assignment.ElderId &&
                             a.Id != assignmentId &&
                             a.Status == "PENDING",
                        cancelUpdate
                    );

                    assignment.Status = "APPROVED";
                    assignment.ApprovedAt = DateTime.UtcNow;

                    return ApiResponse<ElderCaretakerAssignment>.SuccessResponse(
                        assignment,
                        $"You are now caretaker of {assignment.ElderName}!"
                    );
                }
                else
                {
                    // REJECT
                    var rejectUpdate = Builders<ElderCaretakerAssignment>.Update
                        .Set(a => a.Status, "REJECTED")
                        .Set(a => a.IsActive, false);
                    await _assignments.UpdateOneAsync(a => a.Id == assignmentId, rejectUpdate);

                    assignment.Status = "REJECTED";

                    return ApiResponse<ElderCaretakerAssignment>.SuccessResponse(
                        assignment,
                        "Request rejected"
                    );
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<ElderCaretakerAssignment>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET PENDING REQUESTS (Caretaker)
        public async Task<ApiResponse<List<ElderCaretakerAssignment>>> GetPendingRequestsAsync(string caretakerId)
        {
            try
            {
                var requests = await _assignments.Find(
                    a => a.CaretakerId == caretakerId && a.Status == "PENDING"
                ).SortByDescending(a => a.AssignedAt).ToListAsync();

                return ApiResponse<List<ElderCaretakerAssignment>>.SuccessResponse(
                    requests,
                    $"Found {requests.Count} pending requests"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ElderCaretakerAssignment>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET MY SENT REQUESTS (Elder - to see status)
        public async Task<ApiResponse<List<ElderCaretakerAssignment>>> GetMySentRequestsAsync(string elderId)
        {
            try
            {
                var requests = await _assignments.Find(
                    a => a.ElderId == elderId
                ).SortByDescending(a => a.AssignedAt).ToListAsync();

                return ApiResponse<List<ElderCaretakerAssignment>>.SuccessResponse(
                    requests,
                    $"Found {requests.Count} requests"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ElderCaretakerAssignment>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ASSIGNMENT BY ELDER ID (Only APPROVED)
        public async Task<ApiResponse<ElderCaretakerAssignment>> GetAssignmentByElderIdAsync(string elderId)
        {
            try
            {
                var assignment = await _assignments.Find(
                    a => a.ElderId == elderId && a.Status == "APPROVED" && a.IsActive
                ).FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return ApiResponse<ElderCaretakerAssignment>.ErrorResponse("No caretaker assigned yet");
                }

                return ApiResponse<ElderCaretakerAssignment>.SuccessResponse(assignment);
            }
            catch (Exception ex)
            {
                return ApiResponse<ElderCaretakerAssignment>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET MY ELDERS (Caretaker - only APPROVED)
        public async Task<ApiResponse<List<ElderCaretakerAssignment>>> GetAssignmentsByCaretakerIdAsync(string caretakerId)
        {
            try
            {
                var assignments = await _assignments.Find(
                    a => a.CaretakerId == caretakerId && a.Status == "APPROVED" && a.IsActive
                ).ToListAsync();

                return ApiResponse<List<ElderCaretakerAssignment>>.SuccessResponse(
                    assignments,
                    $"Found {assignments.Count} assigned elders"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ElderCaretakerAssignment>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // REMOVE ASSIGNMENT
        public async Task<ApiResponse<bool>> RemoveAssignmentAsync(string assignmentId)
        {
            try
            {
                var update = Builders<ElderCaretakerAssignment>.Update
                    .Set(a => a.IsActive, false)
                    .Set(a => a.Status, "REMOVED");

                var result = await _assignments.UpdateOneAsync(a => a.Id == assignmentId, update);

                if (result.ModifiedCount > 0)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Assignment removed");
                }

                return ApiResponse<bool>.ErrorResponse("Assignment not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}