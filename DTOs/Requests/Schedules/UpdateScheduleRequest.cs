using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.Schedules
{
    public class UpdateScheduleRequest
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Range(1, 100)]
        public int? Capacity { get; set; }

        public bool? IsActive { get; set; }
    }
}
