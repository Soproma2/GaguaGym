using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.MembershipPlans
{
    public class AssignMembershipRequest
    {
        [Required]
        public int MemberId { get; set; }

        [Required]
        public int PlanId { get; set; }

        public DateTime? StartDate { get; set; }
    }
}
