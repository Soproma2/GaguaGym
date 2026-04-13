using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.Members
{
    public class UpdateMemberRequest
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
    }
}
