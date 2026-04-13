using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.MembershipPlans
{
    public class CreateMembershipPlanRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required, Range(1, 3650)]
        public int DurationDays { get; set; }
    }
}
