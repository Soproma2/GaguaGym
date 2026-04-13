namespace GaguaGym.DTOs.Responses.Member
{
    public class MemberListResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? MembershipStatus { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
