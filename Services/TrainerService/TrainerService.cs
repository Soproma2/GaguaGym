using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.Trainers;
using GaguaGym.DTOs.Responses.Schedule;
using GaguaGym.DTOs.Responses.Trainer;
using GaguaGym.Enums;
using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Services.TrainerService
{
    public class TrainerService : ITrainerService
    {
        private readonly AppDbContext _db;
        public TrainerService(AppDbContext db) => _db = db;

        public Result<List<TrainerResponse>> GetAll()
        {
            var trainers = _db.Trainers
                .Include(t => t.User)
                .Where(t => t.User.IsActive)
                .OrderBy(t => t.User.FirstName)
                .ToList()
                .Select(Mappers.ToTrainerResponse)
                .ToList();

            return Result<List<TrainerResponse>>.Success(trainers);
        }

        public Result<TrainerResponse> GetById(int id)
        {
            var trainer = _db.Trainers
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);

            return trainer is null
                ? Result<TrainerResponse>.NotFound("ტრენერი ვერ მოიძებნა.")
                : Result<TrainerResponse>.Success(Mappers.ToTrainerResponse(trainer));
        }

        public Result<TrainerResponse> Create(CreateTrainerRequest req)
        {
            if (_db.Users.Any(u => u.Email == req.Email.ToLower()))
                return Result<TrainerResponse>.Failure("ელ.ფოსტა უკვე გამოყენებულია.");

            var user = new User
            {
                FirstName = req.FirstName.Trim(),
                LastName = req.LastName.Trim(),
                Email = req.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12),
                Role = UserRole.Trainer,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            var trainer = new Trainer
            {
                UserId = user.Id,
                Specialization = req.Specialization.Trim(),
                Bio = req.Bio?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.Trainers.Add(trainer);
            _db.SaveChanges();

            var created = _db.Trainers
                .Include(t => t.User)
                .First(t => t.Id == trainer.Id);

            return Result<TrainerResponse>.Success(Mappers.ToTrainerResponse(created), 201);
        }

        public Result<TrainerResponse> Update(int id, UpdateTrainerRequest req)
        {
            var trainer = _db.Trainers
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);

            if (trainer is null)
                return Result<TrainerResponse>.NotFound("ტრენერი ვერ მოიძებნა.");

            if (req.Specialization is not null) trainer.Specialization = req.Specialization.Trim();
            if (req.Bio is not null) trainer.Bio = req.Bio.Trim();
            if (req.IsAvailable.HasValue) trainer.IsAvailable = req.IsAvailable.Value;

            trainer.User.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            return Result<TrainerResponse>.Success(Mappers.ToTrainerResponse(trainer));
        }

        public Result<List<ScheduleResponse>> GetSchedules(int trainerId)
        {
            if (!_db.Trainers.Any(t => t.Id == trainerId))
                return Result<List<ScheduleResponse>>.NotFound("ტრენერი ვერ მოიძებნა.");

            var schedules = _db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .Where(s => s.TrainerId == trainerId && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToList()
                .Select(Mappers.ToScheduleResponse)
                .ToList();

            return Result<List<ScheduleResponse>>.Success(schedules);
        }
    }
}
