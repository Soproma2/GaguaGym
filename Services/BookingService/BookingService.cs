using GaguaGym.Common;
using GaguaGym.Data;
using GaguaGym.DTOs.Requests.Bookings;
using GaguaGym.DTOs.Responses.Booking;
using GaguaGym.Enums;
using GaguaGym.Models;
using Microsoft.EntityFrameworkCore;

namespace GaguaGym.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _db;
        public BookingService(AppDbContext db) => _db = db;

        public Result<BookingResponse> Create(int memberId, CreateBookingRequest req)
        {
            if (!_db.Members.Any(m => m.Id == memberId))
                return Result<BookingResponse>.NotFound("წევრი ვერ მოიძებნა.");

            var hasActiveMembership = _db.MemberMemberships.Any(ms =>
                ms.MemberId == memberId &&
                ms.Status == MembershipStatus.Active &&
                ms.EndDate > DateTime.UtcNow);

            if (!hasActiveMembership)
                return Result<BookingResponse>.Failure("წევრს არ აქვს აქტიური წევრობა.");

            var schedule = _db.Schedules
                .Include(s => s.Trainer).ThenInclude(t => t.User)
                .FirstOrDefault(s => s.Id == req.ScheduleId && s.IsActive);

            if (schedule is null)
                return Result<BookingResponse>.NotFound("ტრენინგი ვერ მოიძებნა.");

            if (schedule.CurrentCount >= schedule.Capacity)
                return Result<BookingResponse>.Failure("ტრენინგი სავსეა.");

            if (schedule.StartTime <= DateTime.UtcNow)
                return Result<BookingResponse>.Failure("ტრენინგი უკვე დაწყებულია ან დასრულდა.");

            if (_db.Bookings.Any(b =>
                b.MemberId == memberId &&
                b.ScheduleId == req.ScheduleId &&
                b.Status != BookingStatus.Cancelled))
                return Result<BookingResponse>.Failure("ამ ტრენინგზე უკვე ხართ ჩარიცხული.");

            var booking = new Booking
            {
                MemberId = memberId,
                ScheduleId = req.ScheduleId,
                Status = BookingStatus.Confirmed,
                BookedAt = DateTime.UtcNow
            };

            schedule.CurrentCount++;
            _db.Bookings.Add(booking);
            _db.SaveChanges();

            var created = _db.Bookings
                .Include(b => b.Member).ThenInclude(m => m.User)
                .Include(b => b.Schedule).ThenInclude(s => s.Trainer).ThenInclude(t => t.User)
                .First(b => b.Id == booking.Id);

            return Result<BookingResponse>.Success(Mappers.ToBookingResponse(created), 201);
        }

        public Result<bool> Cancel(int bookingId, int memberId, CancelBookingRequest req)
        {
            var booking = _db.Bookings
                .Include(b => b.Schedule)
                .FirstOrDefault(b => b.Id == bookingId && b.MemberId == memberId);

            if (booking is null)
                return Result<bool>.NotFound("ჩაწერა ვერ მოიძებნა.");

            if (booking.Status == BookingStatus.Cancelled)
                return Result<bool>.Failure("ჩაწერა უკვე გაუქმებულია.");

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = req.Reason;
            booking.Schedule.CurrentCount = Math.Max(0, booking.Schedule.CurrentCount - 1);

            _db.SaveChanges();

            return Result<bool>.Success(true);
        }

        public Result<List<BookingResponse>> GetMyBookings(int memberId)
        {
            var bookings = _db.Bookings
                .Include(b => b.Member).ThenInclude(m => m.User)
                .Include(b => b.Schedule).ThenInclude(s => s.Trainer).ThenInclude(t => t.User)
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.BookedAt)
                .ToList()
                .Select(Mappers.ToBookingResponse)
                .ToList();

            return Result<List<BookingResponse>>.Success(bookings);
        }

        public Result<List<BookingResponse>> GetBySchedule(int scheduleId)
        {
            var bookings = _db.Bookings
                .Include(b => b.Member).ThenInclude(m => m.User)
                .Include(b => b.Schedule).ThenInclude(s => s.Trainer).ThenInclude(t => t.User)
                .Where(b => b.ScheduleId == scheduleId && b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.BookedAt)
                .ToList()
                .Select(Mappers.ToBookingResponse)
                .ToList();

            return Result<List<BookingResponse>>.Success(bookings);
        }
    }
}