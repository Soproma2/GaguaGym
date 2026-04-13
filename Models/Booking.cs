using GaguaGym.Enums;

namespace GaguaGym.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public string? CancellationReason { get; set; }

        // Navigation
        public Member Member { get; set; } = null!;
        public Schedule Schedule { get; set; } = null!;
    }
}
