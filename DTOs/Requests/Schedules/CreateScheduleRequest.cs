using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.Schedules
{
    public class CreateScheduleRequest
    {
        [Required]
        public int TrainerId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required, Range(1, 100)]
        public int Capacity { get; set; }
    }
}
