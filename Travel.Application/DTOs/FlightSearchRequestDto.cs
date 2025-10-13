namespace Travel.Application.DTOs
{
    public class FlightSearchRequestDto
    {
        public string Origin { get; set; } = string.Empty;         // IATA e.g., CAI
        public string Destination { get; set; } = string.Empty;    // IATA e.g., DXB
        public DateTime DepartureDate { get; set; }                // YYYY-MM-DD
        public DateTime? ReturnDate { get; set; }                  // optional
        public int Adults { get; set; } = 1;
        public string? Currency { get; set; }                      // optional override
        public int? Max { get; set; }                              // optional override
    }

    public class FlightOfferDto
    {
        public string OfferId { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureAt { get; set; }
        public DateTime ArrivalAt { get; set; }
        public string Airline { get; set; } = string.Empty;        // carrierCode (e.g., EK)
        public string Duration { get; set; } = string.Empty;       // e.g., PT3H25M
        public int Stops { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
    }
    public class PricingResponseDto
    {
        public string OfferId { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Currency { get; set; } = "USD";
        public string Airline { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureAt { get; set; }
        public DateTime ArrivalAt { get; set; }
    }

}
