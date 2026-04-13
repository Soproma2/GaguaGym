using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.Enums;
using GaguaGym.Models;

namespace GaguaGym.Services.MemberService
{
    public class MemberService(AppDbContext db) : IMemberService
    {
        public Result<PagedResult<MemberListResponse>> GetAll(PaginationRequest request)
        {
            var query = db.Members
                .Include(m => m.User)
                .Include(m => m.Memberships.Where(ms => ms.Status == MembershipStatus.Active))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(m =>
                    m.User.FirstName.ToLower().Contains(s) ||
                    m.User.LastName.ToLower().Contains(s) ||
                    m.User.Email.ToLower().Contains(s) ||
                    m.PhoneNumber.Contains(s));
            }

            var total = query.Count();

            var items = query
                .OrderByDescending(m => m.JoinDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new MemberListResponse
                {
                    Id = m.Id,
                    FullName = $"{m.User.FirstName} {m.User.LastName}",
                    Email = m.User.Email,
                    PhoneNumber = m.PhoneNumber,
                    IsActive = m.IsActive,
                    JoinDate = m.JoinDate,
                    MembershipStatus = m.Memberships
                        .Where(ms => ms.Status == MembershipStatus.Active)
                        .Select(ms => ms.Status.ToString())
                        .FirstOrDefault()
                })
                .ToList();

            return Result<PagedResult<MemberListResponse>>.Success(new PagedResult<MemberListResponse>
            {
                Items = items,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Result<MemberResponse> GetById(int id)
        {
            var member = db.Members
                .Include(m => m.User)
                .Include(m => m.Memberships.Where(ms => ms.Status == MembershipStatus.Active))
                    .ThenInclude(ms => ms.Plan)
                .FirstOrDefault(m => m.Id == id);

            return member is null
                ? Result<MemberResponse>.NotFound("წევრი ვერ მოიძებნა.")
                : Result<MemberResponse>.Success(MapToMemberResponse(member));
        }

        public Result<MemberResponse> Create(CreateMemberRequest request)
        {
            if (db.Users.Any(u => u.Email == request.Email.ToLower()))
                return Result<MemberResponse>.Failure("ელ.ფოსტა უკვე გამოყენებულია.");

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                Role = UserRole.Member,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            db.SaveChanges();

            var member = new Member
            {
                UserId = user.Id,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Notes = request.Notes,
                JoinDate = DateTime.UtcNow
            };

            db.Members.Add(member);
            db.SaveChanges();

            var created = db.Members
                .Include(m => m.User)
                .Include(m => m.Memberships)
                .First(m => m.Id == member.Id);

            return Result<MemberResponse>.Success(MapToMemberResponse(created), 201);
        }

        public Result<MemberResponse> Update(int id, UpdateMemberRequest request)
        {
            var member = db.Members
                .Include(m => m.User)
                .Include(m => m.Memberships.Where(ms => ms.Status == MembershipStatus.Active))
                    .ThenInclude(ms => ms.Plan)
                .FirstOrDefault(m => m.Id == id);

            if (member is null)
                return Result<MemberResponse>.NotFound("წევრი ვერ მოიძებნა.");

            if (request.FirstName is not null) member.User.FirstName = request.FirstName.Trim();
            if (request.LastName is not null) member.User.LastName = request.LastName.Trim();
            if (request.PhoneNumber is not null) member.PhoneNumber = request.PhoneNumber;
            if (request.DateOfBirth.HasValue) member.DateOfBirth = request.DateOfBirth;
            if (request.Notes is not null) member.Notes = request.Notes;
            if (request.IsActive.HasValue)
            {
                member.IsActive = request.IsActive.Value;
                member.User.IsActive = request.IsActive.Value;
            }

            member.User.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            return Result<MemberResponse>.Success(MapToMemberResponse(member));
        }

        public Result<bool> Delete(int id)
        {
            var member = db.Members.Include(m => m.User).FirstOrDefault(m => m.Id == id);
            if (member is null)
                return Result<bool>.NotFound("წევრი ვერ მოიძებნა.");

            member.IsActive = false;
            member.User.IsActive = false;
            member.User.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            return Result<bool>.Success(true);
        }

        public Result<List<VisitResponse>> GetVisits(int memberId)
        {
            if (!db.Members.Any(m => m.Id == memberId))
                return Result<List<VisitResponse>>.NotFound("წევრი ვერ მოიძებნა.");

            var visits = db.Visits
                .Include(v => v.Member).ThenInclude(m => m.User)
                .Where(v => v.MemberId == memberId)
                .OrderByDescending(v => v.CheckInTime)
                .ToList()
                .Select(v => MapToVisitResponse(v))
                .ToList();

            return Result<List<VisitResponse>>.Success(visits);
        }

        public Result<List<MemberMembershipResponse>> GetMemberships(int memberId)
        {
            if (!db.Members.Any(m => m.Id == memberId))
                return Result<List<MemberMembershipResponse>>.NotFound("წევრი ვერ მოიძებნა.");

            var memberships = db.MemberMemberships
                .Include(ms => ms.Member).ThenInclude(m => m.User)
                .Include(ms => ms.Plan)
                .Where(ms => ms.MemberId == memberId)
                .OrderByDescending(ms => ms.StartDate)
                .ToList()
                .Select(MapToMemberMembershipResponse)
                .ToList();

            return Result<List<MemberMembershipResponse>>.Success(memberships);
        }

        // ─── Static Mappers (shared across services) ──────────────────────────────

        public static MemberResponse MapToMemberResponse(Member m) => new()
        {
            Id = m.Id,
            UserId = m.UserId,
            FirstName = m.User.FirstName,
            LastName = m.User.LastName,
            Email = m.User.Email,
            PhoneNumber = m.PhoneNumber,
            DateOfBirth = m.DateOfBirth,
            JoinDate = m.JoinDate,
            IsActive = m.IsActive,
            Notes = m.Notes,
            ActiveMembership = m.Memberships
                .Where(ms => ms.Status == MembershipStatus.Active)
                .Select(MapToMemberMembershipResponse)
                .FirstOrDefault()
        };

        public static MemberMembershipResponse MapToMemberMembershipResponse(MemberMembership ms) => new()
        {
            Id = ms.Id,
            MemberId = ms.MemberId,
            MemberFullName = ms.Member?.User is not null
                ? $"{ms.Member.User.FirstName} {ms.Member.User.LastName}" : "",
            Plan = new MembershipPlanResponse
            {
                Id = ms.Plan.Id,
                Name = ms.Plan.Name,
                Description = ms.Plan.Description,
                Price = ms.Plan.Price,
                DurationDays = ms.Plan.DurationDays,
                IsActive = ms.Plan.IsActive
            },
            StartDate = ms.StartDate,
            EndDate = ms.EndDate,
            Status = ms.Status.ToString(),
            PaidAmount = ms.PaidAmount,
            DaysRemaining = ms.Status == MembershipStatus.Active
                ? Math.Max(0, (ms.EndDate - DateTime.UtcNow).Days) : 0
        };

        public static VisitResponse MapToVisitResponse(Visit v)
        {
            string? duration = null;
            if (v.CheckOutTime.HasValue)
            {
                var span = v.CheckOutTime.Value - v.CheckInTime;
                duration = $"{(int)span.TotalHours}სთ {span.Minutes}წთ";
            }
            return new VisitResponse
            {
                Id = v.Id,
                MemberId = v.MemberId,
                MemberFullName = v.Member?.User is not null
                    ? $"{v.Member.User.FirstName} {v.Member.User.LastName}" : "",
                CheckInTime = v.CheckInTime,
                CheckOutTime = v.CheckOutTime,
                Duration = duration,
                Notes = v.Notes
            };
        }
    }
}
