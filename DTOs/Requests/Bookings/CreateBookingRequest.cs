using System.ComponentModel.DataAnnotations;

namespace GaguaGym.DTOs.Requests.Bookings
{
    public class CreateBookingRequest
    {
        [Required]
        public int ScheduleId { get; set; }
    }
}
