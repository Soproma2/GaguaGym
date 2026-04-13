using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.Visits;
using GaguaGym.DTOs.Responses.Visit;
using GaguaGym.Enums;
using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Services.VisitService
{
    public class VisitService(AppDbContext db) : IVisitService
    {
        public Result<VisitResponse> CheckIn(CheckInRequest request)
        {
            var member = db.Members
                .Include(m => m.User)
                .FirstOrDefault(m => m.Id == request.MemberId && m.IsActive);

            if (member is null)
                return Result<VisitResponse>.NotFound("წევრი ვერ მოიძებნა.");

            if (db.Visits.Any(v => v.MemberId == request.MemberId && v.CheckOutTime == null))
                return Result<VisitResponse>.Failure("წევრს უკვე აქვს გახსნილი ვიზიტი.");

            var hasActiveMembership = db.MemberMemberships.Any(ms =>
                ms.MemberId == request.MemberId &&
                ms.Status == MembershipStatus.Active &&
                ms.EndDate > DateTime.UtcNow);

            if (!hasActiveMembership)
                return Result<VisitResponse>.Failure("წევრს არ აქვს მოქმედი წევრობა.");

            var visit = new Visit
            {
                MemberId = request.MemberId,
                CheckInTime = DateTime.UtcNow,
                Notes = request.Notes
            };

            db.Visits.Add(visit);
            db.SaveChanges();

            var created = db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .First(v => v.Id == visit.Id);

            return Result<VisitResponse>.Success(MemberService.MapToVisitResponse(created), 201);
        }

        public Result<VisitResponse> CheckOut(int visitId)
        {
            var visit = db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .FirstOrDefault(v => v.Id == visitId);

            if (visit is null)
                return Result<VisitResponse>.NotFound("ვიზიტი ვერ მოიძებნა.");

            if (visit.CheckOutTime.HasValue)
                return Result<VisitResponse>.Failure("ეს ვიზიტი უკვე დახურულია.");

            visit.CheckOutTime = DateTime.UtcNow;
            db.SaveChanges();

            return Result<VisitResponse>.Success(MemberService.MapToVisitResponse(visit));
        }

        public Result<List<VisitResponse>> GetTodayVisits()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var visits = db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .Where(v => v.CheckInTime.Date == todayUtc)
                .OrderByDescending(v => v.CheckInTime)
                .ToList()
                .Select(MemberService.MapToVisitResponse)
                .ToList();

            return Result<List<VisitResponse>>.Success(visits);
        }

        public Result<List<VisitResponse>> GetMemberVisits(int memberId)
        {
            if (!db.Members.Any(m => m.Id == memberId))
                return Result<List<VisitResponse>>.NotFound("წევრი ვერ მოიძებნა.");

            var visits = db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .Where(v => v.MemberId == memberId)
                .OrderByDescending(v => v.CheckInTime)
                .ToList()
                .Select(MemberService.MapToVisitResponse)
                .ToList();

            return Result<List<VisitResponse>>.Success(visits);
        }
    }
}
