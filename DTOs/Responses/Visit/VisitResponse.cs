namespace GaguaGym.DTOs.Responses.Visit
{
    public class VisitResponse
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string MemberFullName { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? Duration { get; set; }
        public string? Notes { get; set; }
    }
}
