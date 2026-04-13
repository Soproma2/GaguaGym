using GaguaGym.DTOs.Responses.Trainer;

namespace GaguaGym.DTOs.Responses.Schedule
{
    public class ScheduleResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public int CurrentCount { get; set; }
        public int AvailableSpots { get; set; }
        public bool IsActive { get; set; }
        public TrainerResponse Trainer { get; set; } = null!;
    }
}
