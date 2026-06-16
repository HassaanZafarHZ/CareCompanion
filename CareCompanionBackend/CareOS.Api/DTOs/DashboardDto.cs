namespace CareOS.Api.DTOs
{
    public class ElderDashboardStatsDto
    {
        public int TodayMedicationsPending { get; set; }
        public int TodayMedicationsCompleted { get; set; }
        public int UnreadMessages { get; set; }
        public int UpcomingActivities { get; set; }
        public BloodPressureStatus? LatestBPStatus { get; set; }
        public bool HasCompletedTodayCheckIn { get; set; }
        public AssignmentInfo? AssignedCaretaker { get; set; }
    }

    public class CaretakerDashboardStatsDto
    {
        public int TotalAssignedElders { get; set; }
        public int PendingEmergencies { get; set; }
        public int PendingPrescriptions { get; set; }
        public int TasksDueToday { get; set; }
        public int UnreadMessages { get; set; }
        public List<ElderSummary> AssignedElders { get; set; } = new();
    }

    public class BloodPressureStatus
    {
        public int Systolic { get; set; }
        public int Diastolic { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
    }

    public class AssignmentInfo
    {
        public string CaretakerId { get; set; } = string.Empty;
        public string CaretakerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class ElderSummary
    {
        public string ElderId { get; set; } = string.Empty;
        public string ElderName { get; set; } = string.Empty;
        public int PendingMedications { get; set; }
        public bool HasEmergencyAlert { get; set; }
        public DateTime? LastCheckIn { get; set; }
    }
}