using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Travel.Application.DTOs;
using Travel.Application.DTOs.Bookings;
using Travel.Application.Interfaces;

namespace TravelApiProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.CreateBookingAsync(userId, dto);
            return Ok(ApiResponse<BookingResponseDto>.Success(result, "Booking created successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(ApiResponse<IEnumerable<BookingResponseDto>>.Success(result, "Bookings retrieved successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ok = await _bookingService.CancelBookingAsync(id, userId);
            if (!ok)
                return NotFound(ApiResponse<string>.Fail("Booking not found"));

            return Ok(ApiResponse<string>.Success(null, "Booking cancelled successfully"));
        }
    }
}
