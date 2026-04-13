using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.MembershipPlans;
using GaguaGym.DTOs.Responses.Membership;
using GaguaGym.DTOs.Responses.MembershipPlan;
using GaguaGym.Enums;
using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Services.MembershipService
{
    public class MembershipService : IMembershipService
    {
        private readonly AppDbContext _db;
        public MembershipService(AppDbContext db) => _db = db;

        public Result<List<MembershipPlanResponse>> GetAllPlans()
        {
            var plans = _db.MembershipPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToList()
                .Select(Mappers.ToPlanResponse)
                .ToList();

            return Result<List<MembershipPlanResponse>>.Success(plans);
        }

        public Result<MembershipPlanResponse> GetPlanById(int id)
        {
            var plan = _db.MembershipPlans.Find(id);

            return plan is null
                ? Result<MembershipPlanResponse>.NotFound("გეგმა ვერ მოიძებნა.")
                : Result<MembershipPlanResponse>.Success(Mappers.ToPlanResponse(plan));
        }

        public Result<MembershipPlanResponse> CreatePlan(CreateMembershipPlanRequest req)
        {
            var plan = new MembershipPlan
            {
                Name = req.Name.Trim(),
                Description = req.Description?.Trim(),
                Price = req.Price,
                DurationDays = req.DurationDays,
                CreatedAt = DateTime.UtcNow
            };

            _db.MembershipPlans.Add(plan);
            _db.SaveChanges();

            return Result<MembershipPlanResponse>.Success(Mappers.ToPlanResponse(plan), 201);
        }

        public Result<MembershipPlanResponse> UpdatePlan(int id, UpdateMembershipPlanRequest req)
        {
            var plan = _db.MembershipPlans.Find(id);
            if (plan is null)
                return Result<MembershipPlanResponse>.NotFound("გეგმა ვერ მოიძებნა.");

            if (req.Name is not null) plan.Name = req.Name.Trim();
            if (req.Description is not null) plan.Description = req.Description.Trim();
            if (req.Price.HasValue) plan.Price = req.Price.Value;
            if (req.DurationDays.HasValue) plan.DurationDays = req.DurationDays.Value;
            if (req.IsActive.HasValue) plan.IsActive = req.IsActive.Value;

            _db.SaveChanges();

            return Result<MembershipPlanResponse>.Success(Mappers.ToPlanResponse(plan));
        }

        public Result<MembershipResponse> AssignMembership(AssignMembershipRequest req)
        {
            var member = _db.Members
                .Include(m => m.User)
                .FirstOrDefault(m => m.Id == req.MemberId);

            if (member is null)
                return Result<MembershipResponse>.NotFound("წევრი ვერ მოიძებნა.");

            var plan = _db.MembershipPlans.Find(req.PlanId);
            if (plan is null || !plan.IsActive)
                return Result<MembershipResponse>.NotFound("გეგმა ვერ მოიძებნა ან გათიშულია.");

            var existing = _db.MemberMemberships
                .Where(ms => ms.MemberId == req.MemberId && ms.Status == MembershipStatus.Active)
                .ToList();
            existing.ForEach(ms => ms.Status = MembershipStatus.Expired);

            var startDate = req.StartDate?.ToUniversalTime() ?? DateTime.UtcNow;

            var membership = new Membership
            {
                MemberId = req.MemberId,
                PlanId = req.PlanId,
                StartDate = startDate,
                EndDate = startDate.AddDays(plan.DurationDays),
                Status = MembershipStatus.Active,
                PaidAmount = plan.Price,
                CreatedAt = DateTime.UtcNow
            };

            _db.MemberMemberships.Add(membership);
            _db.SaveChanges();

            var created = _db.MemberMemberships
                .Include(ms => ms.Member).ThenInclude(m => m.User)
                .Include(ms => ms.Plan)
                .First(ms => ms.Id == membership.Id);

            return Result<MembershipResponse>.Success(Mappers.ToMemberMembershipResponse(created), 201);
        }

        public Result<bool> CancelMembership(int membershipId)
        {
            var membership = _db.MemberMemberships.Find(membershipId);
            if (membership is null)
                return Result<bool>.NotFound("წევრობა ვერ მოიძებნა.");

            if (membership.Status != MembershipStatus.Active)
                return Result<bool>.Failure("მხოლოდ აქტიური წევრობა შეიძლება გაუქმდეს.");

            membership.Status = MembershipStatus.Cancelled;
            _db.SaveChanges();

            return Result<bool>.Success(true);
        }

        public Result<List<MembershipResponse>> GetActiveMemberships()
        {
            var memberships = _db.MemberMemberships
                .Include(ms => ms.Member).ThenInclude(m => m.User)
                .Include(ms => ms.Plan)
                .Where(ms => ms.Status == MembershipStatus.Active)
                .OrderBy(ms => ms.EndDate)
                .ToList()
                .Select(Mappers.ToMemberMembershipResponse)
                .ToList();

            return Result<List<MembershipResponse>>.Success(memberships);
        }
    }
}
