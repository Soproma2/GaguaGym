using System.ComponentModel;

namespace GaguaGym.Models
{
    public class Member
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }
}
