namespace GaguaGym.DTOs.Responses.Trainer
{
    public class TrainerResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsAvailable { get; set; }
    }
}
