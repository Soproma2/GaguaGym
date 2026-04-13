namespace GaguaGym.Models
{
    public class Visit
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
        public DateTime? CheckOutTime { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Member Member { get; set; } = null!;
    }
}
