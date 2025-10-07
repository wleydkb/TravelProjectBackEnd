namespace Travel.Core.Entities
{
    public class FlightCache
    {
        public int Id { get; set; }
        public string OfferId { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public string Airline { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string RawResponse { get; set; } = string.Empty;
        public DateTime CachedAt { get; set; } = DateTime.UtcNow;
    }
}
