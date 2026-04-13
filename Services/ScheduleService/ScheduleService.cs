using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.Schedules;
using GaguaGym.DTOs.Responses.Schedule;
using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Services.ScheduleService
{
    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _db;
        public ScheduleService(AppDbContext db) => _db = db;

        public Result<List<ScheduleResponse>> GetAll(DateTime? from, DateTime? to)
        {
            var query = _db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .Where(s => s.IsActive)
                .AsQueryable();

            if (from.HasValue) query = query.Where(s => s.StartTime >= from.Value.ToUniversalTime());
            if (to.HasValue) query = query.Where(s => s.StartTime <= to.Value.ToUniversalTime());

            var schedules = query
                .OrderBy(s => s.StartTime)
                .ToList()
                .Select(Mappers.ToScheduleResponse)
                .ToList();

            return Result<List<ScheduleResponse>>.Success(schedules);
        }

        public Result<ScheduleResponse> GetById(int id)
        {
            var schedule = _db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .FirstOrDefault(s => s.Id == id);

            return schedule is null
                ? Result<ScheduleResponse>.NotFound("??????? ??? ????????.")
                : Result<ScheduleResponse>.Success(Mappers.ToScheduleResponse(schedule));
        }

        public Result<ScheduleResponse> Create(CreateScheduleRequest req)
        {
            if (req.EndTime <= req.StartTime)
                return Result<ScheduleResponse>.Failure("?????????? ??? ???? ???? ???????? ????? ?????.");

            if (!_db.Trainers.Any(t => t.Id == req.TrainerId))
                return Result<ScheduleResponse>.NotFound("??????? ??? ????????.");

            var schedule = new Schedule
            {
                TrainerId = req.TrainerId,
                Title = req.Title.Trim(),
                Description = req.Description?.Trim(),
                StartTime = req.StartTime.ToUniversalTime(),
                EndTime = req.EndTime.ToUniversalTime(),
                Capacity = req.Capacity,
                CreatedAt = DateTime.UtcNow
            };

            _db.Schedules.Add(schedule);
            _db.SaveChanges();

            var created = _db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .First(s => s.Id == schedule.Id);

            return Result<ScheduleResponse>.Success(Mappers.ToScheduleResponse(created), 201);
        }

        public Result<ScheduleResponse> Update(int id, UpdateScheduleRequest req)
        {
            var schedule = _db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .FirstOrDefault(s => s.Id == id);

            if (schedule is null)
                return Result<ScheduleResponse>.NotFound("??????? ??? ????????.");

            if (req.Title is not null) schedule.Title = req.Title.Trim();
            if (req.Description is not null) schedule.Description = req.Description.Trim();
            if (req.StartTime.HasValue) schedule.StartTime = req.StartTime.Value.ToUniversalTime();
            if (req.EndTime.HasValue) schedule.EndTime = req.EndTime.Value.ToUniversalTime();
            if (req.Capacity.HasValue) schedule.Capacity = req.Capacity.Value;
            if (req.IsActive.HasValue) schedule.IsActive = req.IsActive.Value;

            _db.SaveChanges();

            return Result<ScheduleResponse>.Success(Mappers.ToScheduleResponse(schedule));
        }

        public Result<bool> Delete(int id)
        {
            var schedule = _db.Schedules.Find(id);
            if (schedule is null)
                return Result<bool>.NotFound("??????? ??? ????????.");

            schedule.IsActive = false;
            _db.SaveChanges();

            return Result<bool>.Success(true);
        }
    }
}
