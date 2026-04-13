using GaguaGym.DTOs.Responses.Schedule;

namespace GaguaGym.DTOs.Responses.Booking
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string MemberFullName { get; set; } = string.Empty;
        public ScheduleResponse Schedule { get; set; } = null!;
        public DateTime BookedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
    }
}
