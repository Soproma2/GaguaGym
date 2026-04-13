using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.Trainers
{
    public class CreateTrainerRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Specialization { get; set; } = string.Empty;

        public string? Bio { get; set; }
    }
}
