using Travel.Application.DTOs.Bookings;

namespace Travel.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(int userId, BookingCreateDto dto);
        Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId);
        Task<bool> CancelBookingAsync(int id, int userId);
    }
}
