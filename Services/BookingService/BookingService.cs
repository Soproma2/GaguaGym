using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.Bookings;
using GaguaGym.DTOs.Responses.Booking;
using GaguaGym.Enums;
using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Services.BookingService
{
    public class BookingService(AppDbContext db) : IBookingService
    {
        public Result<BookingResponse> Create(int memberId, CreateBookingRequest request)
        {
            var member = db.Members.Include(m => m.User).FirstOrDefault(m => m.Id == memberId);
            if (member is null)
                return Result<BookingResponse>.NotFound("წევრი ვერ მოიძებნა.");

            var hasActiveMembership = db.MemberMemberships.Any(ms =>
                ms.MemberId == memberId &&
                ms.Status == MembershipStatus.Active &&
                ms.EndDate > DateTime.UtcNow);

            if (!hasActiveMembership)
                return Result<BookingResponse>.Failure("წევრს არ აქვს აქტიური წევრობა.");

            var schedule = db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .FirstOrDefault(s => s.Id == request.ScheduleId && s.IsActive);

            if (schedule is null)
                return Result<BookingResponse>.NotFound("ტრენინგი ვერ მოიძებნა.");

            if (schedule.CurrentCount >= schedule.Capacity)
                return Result<BookingResponse>.Failure("ტრენინგი სავსეა.");

            if (schedule.StartTime <= DateTime.UtcNow)
                return Result<BookingResponse>.Failure("ტრენინგი უკვე დაწყებულია ან დასრულდა.");

            var alreadyBooked = db.Bookings.Any(b =>
                b.MemberId == memberId &&
                b.ScheduleId == request.ScheduleId &&
                b.Status != BookingStatus.Cancelled);

            if (alreadyBooked)
                return Result<BookingResponse>.Failure("ამ ტრენინგზე უკვე ხართ ჩარიცხული.");

            var booking = new Booking
            {
                MemberId = memberId,
                ScheduleId = request.ScheduleId,
                Status = BookingStatus.Confirmed,
                BookedAt = DateTime.UtcNow
            };

            schedule.CurrentCount++;
            db.Bookings.Add(booking);
            db.SaveChanges();

            var created = db.Bookings
                .Include(b => b.Member).ThenInclude(m => m.User)
                .Include(b => b.Schedule).ThenInclude(s => s.Trainer).ThenInclude(t => t.User)
                .First(b => b.Id == booking.Id);

            return Result<BookingResponse>.Success(MapToResponse(created), 201);
        }

        public Result<bool> Cancel(int bookingId, int memberId, CancelBookingRequest request)
        {
            var booking = db.Bookings
                .Include(b => b.Schedule)
                .FirstOrDefault(b => b.Id == bookingId && b.MemberId == memberId);

            if (booking is null)
                return Result<bool>.NotFound("ჩაწერა ვერ მოიძებნა.");

            if (booking.Status == BookingStatus.Cancelled)
                return Result<bool>.Failure("ჩაწერა უკვე გაუქმებულია.");

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = request.Reason;
            booking.Schedule.CurrentCount = Math.Max(0, booking.Schedule.CurrentCount - 1);

            db.SaveChanges();
            return Result<bool>.Success(true);
        }

        public Result<List<BookingResponse>> GetMyBookings(int memberId)
        {
            var bookings = db.Bookings
                .Include(b => b.Member).ThenInclude(m => m.User)
                .Include(b => b.Schedule).ThenInclude(s => s.Trainer).ThenInclude(t => t.User)
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.BookedAt)
                .ToList()
                .Select(MapToResponse)
                .ToList();

            return Result<List<BookingResponse>>.Success(bookings);
        }

        public Result<List<BookingResponse>> GetBySchedule(int scheduleId)
        {
            var bookings = db.Bookings
                .Include(b => b.Member).ThenInclude(m => m.User)
                .Include(b => b.Schedule).ThenInclude(s => s.Trainer).ThenInclude(t => t.User)
                .Where(b => b.ScheduleId == scheduleId && b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.BookedAt)
                .ToList()
                .Select(MapToResponse)
                .ToList();

            return Result<List<BookingResponse>>.Success(bookings);
        }

        private static BookingResponse MapToResponse(Booking b) => new()
        {
            Id = b.Id,
            MemberId = b.MemberId,
            MemberFullName = $"{b.Member.User.FirstName} {b.Member.User.LastName}",
            Schedule = ScheduleService.MapToResponse(b.Schedule),
            BookedAt = b.BookedAt,
            Status = b.Status.ToString(),
            CancellationReason = b.CancellationReason
        };
    }
}
