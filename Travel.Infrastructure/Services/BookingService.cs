using Microsoft.EntityFrameworkCore;
using Travel.Application.DTOs.Bookings;
using Travel.Application.Interfaces;
using Travel.Core.Entities;
using Travel.Infrastructure.Data;

namespace Travel.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _db;
        private readonly IAmadeusService _amadeus;

        public BookingService(AppDbContext db, IAmadeusService amadeus)
        {
            _db = db;
            _amadeus = amadeus;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(int userId, BookingCreateDto dto)
        {
            // Get offer from cache
            var cache = await _db.FlightCaches.FirstOrDefaultAsync(x => x.OfferId == dto.OfferId);
            if (cache == null)
                throw new Exception("Flight offer not found or expired");

            // Confirm price
            var priceInfo = await _amadeus.GetFlightPriceAsync(dto.OfferId);
            if (priceInfo == null)
                throw new Exception("Could not confirm price");

            var booking = new Booking
            {
                UserId = userId,
                FlightOfferId = dto.OfferId,
                DepartureDate = cache.DepartureDate,
                Passengers = dto.Passengers,
                TotalPrice = priceInfo.Total * dto.Passengers,
                Currency = priceInfo.Currency,
                Status = "Pending",
                RawFlightData = cache.RawResponse
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return new BookingResponseDto
            {
                Id = booking.Id,
                FlightOfferId = booking.FlightOfferId,
                TotalPrice = booking.TotalPrice,
                Currency = booking.Currency,
                Origin = cache.Origin,
                Destination = cache.Destination,
                Airline = cache.Airline,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt
            };
        }

        public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _db.Bookings
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                FlightOfferId = b.FlightOfferId,
                TotalPrice = b.TotalPrice,
                Currency = b.Currency,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            });
        }

        public async Task<bool> CancelBookingAsync(int id, int userId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (booking == null) return false;

            booking.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
