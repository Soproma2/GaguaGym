using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Bookings;
using GaguaGym.DTOs.Responses.Booking;

namespace GaguaGym.Services.BookingService
{
    public interface IBookingService
    {
        Result<BookingResponse> Create(int memberId, CreateBookingRequest request);
        Result<bool> Cancel(int bookingId, int memberId, CancelBookingRequest request);
        Result<List<BookingResponse>> GetMyBookings(int memberId);
        Result<List<BookingResponse>> GetBySchedule(int scheduleId);
    }
}
