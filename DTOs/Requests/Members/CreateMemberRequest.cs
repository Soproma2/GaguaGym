using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.Members
{
    public class CreateMemberRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }
        public string? Notes { get; set; }
    }
}
