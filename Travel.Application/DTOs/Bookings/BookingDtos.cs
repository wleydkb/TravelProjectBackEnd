namespace Travel.Application.DTOs.Bookings
{
    public class BookingCreateDto
    {
        public string OfferId { get; set; } = string.Empty;
        public int Passengers { get; set; } = 1;
    }

    public class BookingResponseDto
    {
        public int Id { get; set; }
        public string FlightOfferId { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "USD";
        public string Airline { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
