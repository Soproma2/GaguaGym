using GaguaGym.Common;
using GaguaGym.DTOs.Requests.Bookings;
using GaguaGym.DTOs.Responses.Booking;
using GaguaGym.Services.BookingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GaguaGym.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingsController(IBookingService bookingService) => _bookingService = bookingService;

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpPost]
        [Authorize(Roles = "Member,Admin")]
        [ProducesResponseType(typeof(Result<BookingResponse>), 201)]
        [ProducesResponseType(typeof(Result<BookingResponse>), 400)]
        [ProducesResponseType(typeof(Result<BookingResponse>), 404)]
        public IActionResult Create(CreateBookingRequest req)
        {
            var result = _bookingService.Create(CurrentUserId, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Member,Admin")]
        [ProducesResponseType(typeof(Result<bool>), 200)]
        [ProducesResponseType(typeof(Result<bool>), 400)]
        [ProducesResponseType(typeof(Result<bool>), 404)]
        public IActionResult Cancel(int id, CancelBookingRequest req)
        {
            var result = _bookingService.Cancel(id, CurrentUserId, req);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(Result<List<BookingResponse>>), 200)]
        public IActionResult GetMyBookings()
        {
            var result = _bookingService.GetMyBookings(CurrentUserId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("schedule/{scheduleId:int}")]
        [Authorize(Roles = "Admin,Trainer")]
        [ProducesResponseType(typeof(Result<List<BookingResponse>>), 200)]
        [ProducesResponseType(typeof(Result<List<BookingResponse>>), 404)]
        public IActionResult GetBySchedule(int scheduleId)
        {
            var result = _bookingService.GetBySchedule(scheduleId);
            return StatusCode(result.StatusCode, result);
        }
    }

}
