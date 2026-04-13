using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.Visits
{
    public class CheckInRequest
    {
        [Required]
        public int MemberId { get; set; }

        public string? Notes { get; set; }
    }
}
