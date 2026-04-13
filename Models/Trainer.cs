namespace GaguaGym.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
