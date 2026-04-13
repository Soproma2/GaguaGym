using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.Members;
using GaguaGym.DTOs.Requests.PaginationRequest;
using GaguaGym.DTOs.Responses.Member;
using GaguaGym.DTOs.Responses.Membership;
using GaguaGym.DTOs.Responses.MembershipPlan;
using GaguaGym.DTOs.Responses.Visit;
using GaguaGym.Enums;
using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Services.MemberService
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _db;
        public MemberService(AppDbContext db) => _db = db;

        public Result<PagedResult<MemberListResponse>> GetAll(PaginationRequest req)
        {
            var query = _db.Members
                .Include(m => m.User)
                .Include(m => m.Memberships.Where(ms => ms.Status == MembershipStatus.Active))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.Search))
            {
                var s = req.Search.ToLower();
                query = query.Where(m =>
                    m.User.FirstName.ToLower().Contains(s) ||
                    m.User.LastName.ToLower().Contains(s) ||
                    m.User.Email.ToLower().Contains(s) ||
                    m.PhoneNumber.Contains(s));
            }

            var total = query.Count();

            var items = query
                .OrderByDescending(m => m.JoinDate)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
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
                Page = req.Page,
                PageSize = req.PageSize
            });
        }

        public Result<MemberResponse> GetById(int id)
        {
            var member = _db.Members
                .Include(m => m.User)
                .Include(m => m.Memberships.Where(ms => ms.Status == MembershipStatus.Active))
                    .ThenInclude(ms => ms.Plan)
                .FirstOrDefault(m => m.Id == id);

            return member is null
                ? Result<MemberResponse>.NotFound("წევრი ვერ მოიძებნა.")
                : Result<MemberResponse>.Success(Mappers.ToMemberResponse(member));
        }

        public Result<MemberResponse> Create(CreateMemberRequest req)
        {
            if (_db.Users.Any(u => u.Email == req.Email.ToLower()))
                return Result<MemberResponse>.Failure("ელ.ფოსტა უკვე გამოყენებულია.");

            var user = new User
            {
                FirstName = req.FirstName.Trim(),
                LastName = req.LastName.Trim(),
                Email = req.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12),
                Role = UserRole.Member,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            var member = new Member
            {
                UserId = user.Id,
                PhoneNumber = req.PhoneNumber,
                DateOfBirth = req.DateOfBirth,
                Notes = req.Notes,
                JoinDate = DateTime.UtcNow
            };

            _db.Members.Add(member);
            _db.SaveChanges();

            var created = _db.Members
                .Include(m => m.User)
                .Include(m => m.Memberships)
                .First(m => m.Id == member.Id);

            return Result<MemberResponse>.Success(Mappers.ToMemberResponse(created), 201);
        }

        public Result<MemberResponse> Update(int id, UpdateMemberRequest req)
        {
            var member = _db.Members
                .Include(m => m.User)
                .Include(m => m.Memberships.Where(ms => ms.Status == MembershipStatus.Active))
                    .ThenInclude(ms => ms.Plan)
                .FirstOrDefault(m => m.Id == id);

            if (member is null)
                return Result<MemberResponse>.NotFound("წევრი ვერ მოიძებნა.");

            if (req.FirstName is not null) member.User.FirstName = req.FirstName.Trim();
            if (req.LastName is not null) member.User.LastName = req.LastName.Trim();
            if (req.PhoneNumber is not null) member.PhoneNumber = req.PhoneNumber;
            if (req.DateOfBirth.HasValue) member.DateOfBirth = req.DateOfBirth;
            if (req.Notes is not null) member.Notes = req.Notes;
            if (req.IsActive.HasValue)
            {
                member.IsActive = req.IsActive.Value;
                member.User.IsActive = req.IsActive.Value;
            }

            member.User.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            return Result<MemberResponse>.Success(Mappers.ToMemberResponse(member));
        }

        public Result<bool> Delete(int id)
        {
            var member = _db.Members
                .Include(m => m.User)
                .FirstOrDefault(m => m.Id == id);

            if (member is null)
                return Result<bool>.NotFound("წევრი ვერ მოიძებნა.");

            member.IsActive = false;
            member.User.IsActive = false;
            member.User.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            return Result<bool>.Success(true);
        }

        public Result<List<VisitResponse>> GetVisits(int memberId)
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

        public Result<List<MembershipResponse>> GetMemberships(int memberId)
        {
            if (!_db.Members.Any(m => m.Id == memberId))
                return Result<List<MembershipResponse>>.NotFound("წევრი ვერ მოიძებნა.");

            var memberships = _db.MemberMemberships
                .Include(ms => ms.Member).ThenInclude(m => m.User)
                .Include(ms => ms.Plan)
                .Where(ms => ms.MemberId == memberId)
                .OrderByDescending(ms => ms.StartDate)
                .ToList()
                .Select(Mappers.ToMemberMembershipResponse)
                .ToList();

            return Result<List<MembershipResponse>>.Success(memberships);
        }
    }
}
