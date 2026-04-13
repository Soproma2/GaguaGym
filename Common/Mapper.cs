using GaguaGym.DTOs.Responses.Booking;
using GaguaGym.DTOs.Responses.Member;
using GaguaGym.DTOs.Responses.Membership;
using GaguaGym.DTOs.Responses.MembershipPlan;
using GaguaGym.DTOs.Responses.Schedule;
using GaguaGym.DTOs.Responses.Trainer;
using GaguaGym.DTOs.Responses.Visit;
using GaguaGym.Enums;
using GaguaGym.Models;

namespace GaguaGym.Common
{
    public class Mappers
    {
        public static MemberResponse ToMemberResponse(Member m) => new()
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
            .Select(ToMemberMembershipResponse)
            .FirstOrDefault()
        };

        public static MembershipResponse ToMemberMembershipResponse(Membership ms) => new()
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

        public static VisitResponse ToVisitResponse(Visit v)
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

        public static TrainerResponse ToTrainerResponse(Trainer t) => new()
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

        public static ScheduleResponse ToScheduleResponse(Schedule s) => new()
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
            Trainer = ToTrainerResponse(s.Trainer)
        };

        public static MembershipPlanResponse ToPlanResponse(MembershipPlan p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DurationDays = p.DurationDays,
            IsActive = p.IsActive
        };

        public static BookingResponse ToBookingResponse(Booking b) => new()
        {
            Id = b.Id,
            MemberId = b.MemberId,
            MemberFullName = $"{b.Member.User.FirstName} {b.Member.User.LastName}",
            Schedule = ToScheduleResponse(b.Schedule),
            BookedAt = b.BookedAt,
            Status = b.Status.ToString(),
            CancellationReason = b.CancellationReason
        };
    }
}
