using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<Medication> _medications;
        private readonly IMongoCollection<User> _users;

        public MedicationService(MongoDbContext context)
        {
            _context = context;
            _medications = _context.GetCollection<Medication>("Medications");
            _users = _context.GetCollection<User>("Users");
        }

        // CREATE MEDICATION (Caretaker creates schedule for Elder)
        public async Task<ApiResponse<Medication>> CreateMedicationAsync(CreateMedicationDto request, string caretakerId)
        {
            try
            {
                // Verify elder exists
                var elder = await _users.Find(u => u.Id == request.ElderId && u.Role == "ELDER").FirstOrDefaultAsync();
                if (elder == null)
                {
                    return ApiResponse<Medication>.ErrorResponse("Elder not found");
                }

                // Create schedules from time list
                var schedules = request.ScheduleTimes.Select(time => new MedicationSchedule
                {
                    Time = time,
                    IsTaken = false,
                    Day = "Everyday"
                }).ToList();

                var medication = new Medication
                {
                    ElderId = request.ElderId,
                    CaretakerId = caretakerId,
                    MedicineName = request.MedicineName,
                    Dosage = request.Dosage,
                    Frequency = request.Frequency,
                    Schedules = schedules,
                    PrescriptionImage = request.PrescriptionImage,
                    AiSuggested = false,
                    ApprovedByCaretaker = true, // Caretaker ne khud create kiya
                    StartDate = DateTime.UtcNow,
                    EndDate = request.EndDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _medications.InsertOneAsync(medication);

                return ApiResponse<Medication>.SuccessResponse(medication, "Medication schedule created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<Medication>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // APPROVE MEDICATION (Caretaker approves AI-suggested medication)
        public async Task<ApiResponse<Medication>> ApproveMedicationAsync(ApproveMedicationDto request)
        {
            try
            {
                var medication = await _medications.Find(m => m.Id == request.MedicationId).FirstOrDefaultAsync();

                if (medication == null)
                {
                    return ApiResponse<Medication>.ErrorResponse("Medication not found");
                }

                var update = Builders<Medication>.Update
                    .Set(m => m.ApprovedByCaretaker, request.IsApproved)
                    .Set(m => m.IsActive, request.IsApproved);

                await _medications.UpdateOneAsync(m => m.Id == request.MedicationId, update);

                medication.ApprovedByCaretaker = request.IsApproved;
                medication.IsActive = request.IsApproved;

                string message = request.IsApproved
                    ? "Medication approved successfully"
                    : "Medication rejected";

                return ApiResponse<Medication>.SuccessResponse(medication, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<Medication>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // CONFIRM MEDICATION TAKEN (Elder confirms)
        public async Task<ApiResponse<bool>> ConfirmMedicationTakenAsync(ConfirmMedicationDto request)
        {
            try
            {
                var medication = await _medications.Find(m => m.Id == request.MedicationId).FirstOrDefaultAsync();

                if (medication == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Medication not found");
                }

                // Find the schedule and mark as taken
                var schedule = medication.Schedules.FirstOrDefault(s => s.Time == request.ScheduleTime);

                if (schedule == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Schedule time not found");
                }

                schedule.IsTaken = true;
                schedule.TakenAt = request.TakenAt;

                // Update in database
                var update = Builders<Medication>.Update.Set(m => m.Schedules, medication.Schedules);
                await _medications.UpdateOneAsync(m => m.Id == request.MedicationId, update);

                return ApiResponse<bool>.SuccessResponse(true, "Medication marked as taken");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET ELDER'S MEDICATIONS
        public async Task<ApiResponse<List<Medication>>> GetElderMedicationsAsync(string elderId)
        {
            try
            {
                var medications = await _medications
                    .Find(m => m.ElderId == elderId && m.IsActive)
                    .ToListAsync();

                return ApiResponse<List<Medication>>.SuccessResponse(medications, $"Found {medications.Count} medications");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Medication>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET PENDING APPROVALS (AI-suggested medications waiting for approval)
        public async Task<ApiResponse<List<Medication>>> GetPendingApprovalsAsync(string caretakerId)
        {
            try
            {
                var pendingMeds = await _medications
                    .Find(m => m.CaretakerId == caretakerId && m.AiSuggested && !m.ApprovedByCaretaker)
                    .ToListAsync();

                return ApiResponse<List<Medication>>.SuccessResponse(pendingMeds, $"Found {pendingMeds.Count} pending approvals");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Medication>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // GET MEDICATION BY ID
        public async Task<ApiResponse<Medication>> GetMedicationByIdAsync(string medicationId)
        {
            try
            {
                var medication = await _medications.Find(m => m.Id == medicationId).FirstOrDefaultAsync();

                if (medication == null)
                {
                    return ApiResponse<Medication>.ErrorResponse("Medication not found");
                }

                return ApiResponse<Medication>.SuccessResponse(medication);
            }
            catch (Exception ex)
            {
                return ApiResponse<Medication>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // DELETE MEDICATION
        public async Task<ApiResponse<bool>> DeleteMedicationAsync(string medicationId)
        {
            try
            {
                var update = Builders<Medication>.Update.Set(m => m.IsActive, false);
                var result = await _medications.UpdateOneAsync(m => m.Id == medicationId, update);

                if (result.ModifiedCount > 0)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Medication deleted successfully");
                }

                return ApiResponse<bool>.ErrorResponse("Medication not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}