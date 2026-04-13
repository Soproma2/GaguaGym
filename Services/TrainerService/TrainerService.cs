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
    public class TrainerService(AppDbContext db) : ITrainerService
    {
        public Result<List<TrainerResponse>> GetAll()
        {
            var trainers = db.Trainers
                .Include(t => t.User)
                .Where(t => t.User.IsActive)
                .OrderBy(t => t.User.FirstName)
                .ToList()
                .Select(t => MapToResponse(t))
                .ToList();

            return Result<List<TrainerResponse>>.Success(trainers);
        }

        public Result<TrainerResponse> GetById(int id)
        {
            var trainer = db.Trainers.Include(t => t.User).FirstOrDefault(t => t.Id == id);
            return trainer is null
                ? Result<TrainerResponse>.NotFound("ტრენერი ვერ მოიძებნა.")
                : Result<TrainerResponse>.Success(MapToResponse(trainer));
        }

        public Result<TrainerResponse> Create(CreateTrainerRequest request)
        {
            if (db.Users.Any(u => u.Email == request.Email.ToLower()))
                return Result<TrainerResponse>.Failure("ელ.ფოსტა უკვე გამოყენებულია.");

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                Role = UserRole.Trainer,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            db.SaveChanges();

            var trainer = new Trainer
            {
                UserId = user.Id,
                Specialization = request.Specialization.Trim(),
                Bio = request.Bio?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            db.Trainers.Add(trainer);
            db.SaveChanges();

            var created = db.Trainers.Include(t => t.User).First(t => t.Id == trainer.Id);
            return Result<TrainerResponse>.Success(MapToResponse(created), 201);
        }

        public Result<TrainerResponse> Update(int id, UpdateTrainerRequest request)
        {
            var trainer = db.Trainers.Include(t => t.User).FirstOrDefault(t => t.Id == id);
            if (trainer is null)
                return Result<TrainerResponse>.NotFound("ტრენერი ვერ მოიძებნა.");

            if (request.Specialization is not null) trainer.Specialization = request.Specialization.Trim();
            if (request.Bio is not null) trainer.Bio = request.Bio.Trim();
            if (request.IsAvailable.HasValue) trainer.IsAvailable = request.IsAvailable.Value;

            trainer.User.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            return Result<TrainerResponse>.Success(MapToResponse(trainer));
        }

        public Result<List<ScheduleResponse>> GetSchedules(int trainerId)
        {
            if (!db.Trainers.Any(t => t.Id == trainerId))
                return Result<List<ScheduleResponse>>.NotFound("ტრენერი ვერ მოიძებნა.");

            var schedules = db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .Where(s => s.TrainerId == trainerId && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToList()
                .Select(s => ScheduleService.MapToResponse(s))
                .ToList();

            return Result<List<ScheduleResponse>>.Success(schedules);
        }

        public static TrainerResponse MapToResponse(Trainer t) => new()
        {
            Id = t.Id,
            UserId = t.UserId,
            FirstName = t.User.FirstName,
            LastName = t.User.LastName,
            Email = t.User.Email,
            Specialization = t.Specialization,
            Bio = t.Bio,
            IsAvailable = t.IsAvailable
        };
    }
}
