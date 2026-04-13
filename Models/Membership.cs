using GaguaGym.Enums;

namespace GaguaGym.Models
{
    public class Membership
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public MembershipStatus Status { get; set; } = MembershipStatus.Active;
        public decimal PaidAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Member Member { get; set; } = null!;
        public MembershipPlan Plan { get; set; } = null!;
    }
}
