using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.Visits;
using GaguaGym.DTOs.Responses.Visit;
using GaguaGym.Enums;
using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Services.VisitService
{
    public class VisitService : IVisitService
    {
        private readonly AppDbContext _db;
        public VisitService(AppDbContext db) => _db = db;

        public Result<VisitResponse> CheckIn(CheckInRequest req)
        {
            var member = _db.Members
                .Include(m => m.User)
                .FirstOrDefault(m => m.Id == req.MemberId && m.IsActive);

            if (member is null)
                return Result<VisitResponse>.NotFound("წევრი ვერ მოიძებნა.");

            if (_db.Visits.Any(v => v.MemberId == req.MemberId && v.CheckOutTime == null))
                return Result<VisitResponse>.Failure("წევრს უკვე აქვს გახსნილი ვიზიტი.");

            if (!_db.MemberMemberships.Any(ms =>
                ms.MemberId == req.MemberId &&
                ms.Status == MembershipStatus.Active &&
                ms.EndDate > DateTime.UtcNow))
                return Result<VisitResponse>.Failure("წევრს არ აქვს მოქმედი წევრობა.");

            var visit = new Visit
            {
                MemberId = req.MemberId,
                CheckInTime = DateTime.UtcNow,
                Notes = req.Notes
            };

            _db.Visits.Add(visit);
            _db.SaveChanges();

            var created = _db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .First(v => v.Id == visit.Id);

            return Result<VisitResponse>.Success(Mappers.ToVisitResponse(created), 201);
        }

        public Result<VisitResponse> CheckOut(int visitId)
        {
            var visit = _db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .FirstOrDefault(v => v.Id == visitId);

            if (visit is null)
                return Result<VisitResponse>.NotFound("ვიზიტი ვერ მოიძებნა.");

            if (visit.CheckOutTime.HasValue)
                return Result<VisitResponse>.Failure("ეს ვიზიტი უკვე დახურულია.");

            visit.CheckOutTime = DateTime.UtcNow;
            _db.SaveChanges();

            return Result<VisitResponse>.Success(Mappers.ToVisitResponse(visit));
        }

        public Result<List<VisitResponse>> GetTodayVisits()
        {
            var todayUtc = DateTime.UtcNow.Date;

            var visits = _db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .Where(v => v.CheckInTime.Date == todayUtc)
                .OrderByDescending(v => v.CheckInTime)
                .ToList()
                .Select(Mappers.ToVisitResponse)
                .ToList();

            return Result<List<VisitResponse>>.Success(visits);
        }

        public Result<List<VisitResponse>> GetMemberVisits(int memberId)
        {
            if (!_db.Members.Any(m => m.Id == memberId))
                return Result<List<VisitResponse>>.NotFound("წევრი ვერ მოიძებნა.");

            var visits = _db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .Where(v => v.MemberId == memberId)
                .OrderByDescending(v => v.CheckInTime)
                .ToList()
                .Select(Mappers.ToVisitResponse)
                .ToList();

            return Result<List<VisitResponse>>.Success(visits);
        }
    }
}
