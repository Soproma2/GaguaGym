using GaguaGym.DTOs.Responses.MembershipPlan;

namespace GaguaGym.DTOs.Responses.Membership
{
    public class MembershipResponse
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string MemberFullName { get; set; } = string.Empty;
        public MembershipPlanResponse Plan { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public int DaysRemaining { get; set; }
    }
}
