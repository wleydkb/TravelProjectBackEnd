namespace Travel.Core.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        // FlightOfferId من Amadeus (أو أي معرف خارجي للعرض)
        public string FlightOfferId { get; set; } = string.Empty;

        public DateTime DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public int Passengers { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "USD";

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public string? RawFlightData { get; set; }
    }
}
