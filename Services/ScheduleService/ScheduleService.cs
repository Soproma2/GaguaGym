using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.Models;

namespace GaguaGym.Services.ScheduleService
{
    public class ScheduleService(AppDbContext db) : IScheduleService
    {
        public Result<List<ScheduleResponse>> GetAll(DateTime? from, DateTime? to)
        {
            var query = db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .Where(s => s.IsActive)
                .AsQueryable();

            if (from.HasValue) query = query.Where(s => s.StartTime >= from.Value.ToUniversalTime());
            if (to.HasValue) query = query.Where(s => s.StartTime <= to.Value.ToUniversalTime());

            var schedules = query
                .OrderBy(s => s.StartTime)
                .ToList()
                .Select(s => MapToResponse(s))
                .ToList();

            return Result<List<ScheduleResponse>>.Success(schedules);
        }

        public Result<ScheduleResponse> GetById(int id)
        {
            var schedule = db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .FirstOrDefault(s => s.Id == id);

            return schedule is null
                ? Result<ScheduleResponse>.NotFound("განრიგი ვერ მოიძებნა.")
                : Result<ScheduleResponse>.Success(MapToResponse(schedule));
        }

        public Result<ScheduleResponse> Create(CreateScheduleRequest request)
        {
            if (request.EndTime <= request.StartTime)
                return Result<ScheduleResponse>.Failure("დასრულების დრო უნდა იყოს დაწყების დროზე გვიან.");

            if (!db.Trainers.Any(t => t.Id == request.TrainerId))
                return Result<ScheduleResponse>.NotFound("ტრენერი ვერ მოიძებნა.");

            var schedule = new Schedule
            {
                TrainerId = request.TrainerId,
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                StartTime = request.StartTime.ToUniversalTime(),
                EndTime = request.EndTime.ToUniversalTime(),
                Capacity = request.Capacity,
                CreatedAt = DateTime.UtcNow
            };

            db.Schedules.Add(schedule);
            db.SaveChanges();

            var created = db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .First(s => s.Id == schedule.Id);

            return Result<ScheduleResponse>.Success(MapToResponse(created), 201);
        }

        public Result<ScheduleResponse> Update(int id, UpdateScheduleRequest request)
        {
            var schedule = db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .FirstOrDefault(s => s.Id == id);

            if (schedule is null)
                return Result<ScheduleResponse>.NotFound("განრიგი ვერ მოიძებნა.");

            if (request.Title is not null) schedule.Title = request.Title.Trim();
            if (request.Description is not null) schedule.Description = request.Description.Trim();
            if (request.StartTime.HasValue) schedule.StartTime = request.StartTime.Value.ToUniversalTime();
            if (request.EndTime.HasValue) schedule.EndTime = request.EndTime.Value.ToUniversalTime();
            if (request.Capacity.HasValue) schedule.Capacity = request.Capacity.Value;
            if (request.IsActive.HasValue) schedule.IsActive = request.IsActive.Value;

            db.SaveChanges();
            return Result<ScheduleResponse>.Success(MapToResponse(schedule));
        }

        public Result<bool> Delete(int id)
        {
            var schedule = db.Schedules.Find(id);
            if (schedule is null)
                return Result<bool>.NotFound("განრიგი ვერ მოიძებნა.");

            schedule.IsActive = false;
            db.SaveChanges();
            return Result<bool>.Success(true);
        }

        public static ScheduleResponse MapToResponse(Schedule s) => new()
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Capacity = s.Capacity,
            CurrentCount = s.CurrentCount,
            AvailableSpots = s.Capacity - s.CurrentCount,
            IsActive = s.IsActive,
            Trainer = TrainerService.MapToResponse(s.Trainer)
        };
    }
}
