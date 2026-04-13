using GaguaGym.DTOs.Responses.Membership;

namespace GaguaGym.DTOs.Responses.Member
{
    public class MemberResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public MembershipResponse? ActiveMembership { get; set; }
    }
}
