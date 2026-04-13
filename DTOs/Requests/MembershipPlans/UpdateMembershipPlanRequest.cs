using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.MembershipPlans
{
    public class UpdateMembershipPlanRequest
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(1, 3650)]
        public int? DurationDays { get; set; }

        public bool? IsActive { get; set; }
    }
}
