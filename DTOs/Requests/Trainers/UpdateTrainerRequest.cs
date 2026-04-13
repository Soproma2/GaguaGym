using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.Trainers
{
    public class UpdateTrainerRequest
    {
        [MaxLength(200)]
        public string? Specialization { get; set; }

        public string? Bio { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
