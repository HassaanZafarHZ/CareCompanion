using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Models;
using MongoDB.Driver;

namespace CareOS.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Medication> _medications;
        private readonly IMongoCollection<ChatMessage> _messages;
        private readonly IMongoCollection<ActivitySchedule> _activities;
        private readonly IMongoCollection<DailyCheckIn> _checkIns;
        private readonly IMongoCollection<ElderCaretakerAssignment> _assignments;
        private readonly IMongoCollection<EmergencyAlert> _emergencies;
        private readonly IMongoCollection<Prescription> _prescriptions;
        private readonly IMongoCollection<CaretakerTask> _tasks;

        public DashboardService(MongoDbContext context)
        {
            _context = context;
            _users = _context.GetCollection<User>("Users");
            _medications = _context.GetCollection<Medication>("Medications");
            _messages = _context.GetCollection<ChatMessage>("ChatMessages");
            _activities = _context.GetCollection<ActivitySchedule>("ActivitySchedules");
            _checkIns = _context.GetCollection<DailyCheckIn>("DailyCheckIns");
            _assignments = _context.GetCollection<ElderCaretakerAssignment>("Assignments");
            _emergencies = _context.GetCollection<EmergencyAlert>("EmergencyAlerts");
            _prescriptions = _context.GetCollection<Prescription>("Prescriptions");
            _tasks = _context.GetCollection<CaretakerTask>("Tasks");
        }

        // ELDER DASHBOARD STATS
        public async Task<ApiResponse<ElderDashboardStatsDto>> GetElderDashboardStatsAsync(string elderId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                // Get elder info
                var elder = await _users.Find(u => u.Id == elderId).FirstOrDefaultAsync();
                if (elder == null)
                {
                    return ApiResponse<ElderDashboardStatsDto>.ErrorResponse("Elder not found");
                }

                // Today's medications
                var todayMedications = await _medications
                    .Find(m => m.ElderId == elderId && m.IsActive)
                    .ToListAsync();

                var medicationsPending = 0;
                var medicationsCompleted = 0;

                foreach (var med in todayMedications)
                {
                    foreach (var schedule in med.Schedules)
                    {
                        if (schedule.IsTaken)
                            medicationsCompleted++;
                        else
                            medicationsPending++;
                    }
                }

                // Unread messages
                var unreadMessages = await _messages.CountDocumentsAsync(
                    m => m.ReceiverId == elderId && !m.IsRead
                );

                // Upcoming activities
                var upcomingActivities = await _activities.CountDocumentsAsync(
                    a => a.ElderId == elderId && a.Date >= today && !a.IsCompleted
                );

                // Latest BP
                BloodPressureStatus? bpStatus = null;
                if (elder.CurrentBP != null)
                {
                    bpStatus = new BloodPressureStatus
                    {
                        Systolic = elder.CurrentBP.Systolic,
                        Diastolic = elder.CurrentBP.Diastolic,
                        Status = elder.CurrentBP.Status,
                        RecordedAt = elder.CurrentBP.RecordedAt
                    };
                }

                // Today's check-in
                var todayCheckIn = await _checkIns.Find(
                    c => c.ElderId == elderId && c.CheckInTime >= today && c.CheckInTime < tomorrow
                ).FirstOrDefaultAsync();

                // Assigned caretaker
                AssignmentInfo? assignedCaretaker = null;
                var assignment = await _assignments.Find(
                    a => a.ElderId == elderId && a.IsActive
                ).FirstOrDefaultAsync();

                if (assignment != null)
                {
                    var caretaker = await _users.Find(u => u.Id == assignment.CaretakerId).FirstOrDefaultAsync();
                    if (caretaker != null)
                    {
                        assignedCaretaker = new AssignmentInfo
                        {
                            CaretakerId = caretaker.Id,
                            CaretakerName = caretaker.FullName,
                            PhoneNumber = caretaker.PhoneNumber
                        };
                    }
                }

                var stats = new ElderDashboardStatsDto
                {
                    TodayMedicationsPending = medicationsPending,
                    TodayMedicationsCompleted = medicationsCompleted,
                    UnreadMessages = (int)unreadMessages,
                    UpcomingActivities = (int)upcomingActivities,
                    LatestBPStatus = bpStatus,
                    HasCompletedTodayCheckIn = todayCheckIn != null,
                    AssignedCaretaker = assignedCaretaker
                };

                return ApiResponse<ElderDashboardStatsDto>.SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                return ApiResponse<ElderDashboardStatsDto>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // CARETAKER DASHBOARD STATS
        public async Task<ApiResponse<CaretakerDashboardStatsDto>> GetCaretakerDashboardStatsAsync(string caretakerId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                // Total assigned elders
                var assignments = await _assignments
                    .Find(a => a.CaretakerId == caretakerId && a.IsActive)
                    .ToListAsync();

                // Pending emergencies
                var pendingEmergencies = await _emergencies.CountDocumentsAsync(
                    e => e.CaretakerId == caretakerId && !e.IsAcknowledged
                );

                // Pending prescriptions
                var pendingPrescriptions = await _prescriptions.CountDocumentsAsync(
                    p => p.CaretakerId == caretakerId && p.Status == "PENDING"
                );

                // Tasks due today
                var tasksDueToday = await _tasks.CountDocumentsAsync(
                    t => t.CaretakerId == caretakerId &&
                         t.DueDate >= today &&
                         t.DueDate < tomorrow &&
                         !t.IsCompleted
                );

                // Unread messages
                var unreadMessages = await _messages.CountDocumentsAsync(
                    m => m.ReceiverId == caretakerId && !m.IsRead
                );

                // Elder summaries
                var elderSummaries = new List<ElderSummary>();

                foreach (var assignment in assignments)
                {
                    // Pending medications for this elder
                    var elderMedications = await _medications
                        .Find(m => m.ElderId == assignment.ElderId && m.IsActive)
                        .ToListAsync();

                    int pendingMeds = 0;
                    foreach (var med in elderMedications)
                    {
                        pendingMeds += med.Schedules.Count(s => !s.IsTaken);
                    }

                    // Has emergency alert?
                    var hasEmergency = await _emergencies.Find(
                        e => e.ElderId == assignment.ElderId && !e.IsAcknowledged
                    ).AnyAsync();

                    // Last check-in
                    var lastCheckIn = await _checkIns
                        .Find(c => c.ElderId == assignment.ElderId)
                        .SortByDescending(c => c.CheckInTime)
                        .FirstOrDefaultAsync();

                    elderSummaries.Add(new ElderSummary
                    {
                        ElderId = assignment.ElderId,
                        ElderName = assignment.ElderName,
                        PendingMedications = pendingMeds,
                        HasEmergencyAlert = hasEmergency,
                        LastCheckIn = lastCheckIn?.CheckInTime
                    });
                }

                var stats = new CaretakerDashboardStatsDto
                {
                    TotalAssignedElders = assignments.Count,
                    PendingEmergencies = (int)pendingEmergencies,
                    PendingPrescriptions = (int)pendingPrescriptions,
                    TasksDueToday = (int)tasksDueToday,
                    UnreadMessages = (int)unreadMessages,
                    AssignedElders = elderSummaries
                };

                return ApiResponse<CaretakerDashboardStatsDto>.SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                return ApiResponse<CaretakerDashboardStatsDto>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }
}