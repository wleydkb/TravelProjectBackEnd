using Microsoft.AspNetCore.Mvc;
using Travel.Application.DTOs;
using Travel.Application.Interfaces;

namespace TravelApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IAmadeusService _amadeus;

        public FlightsController(IAmadeusService amadeus)
        {
            _amadeus = amadeus;
        }

        // GET: api/flights/search?origin=CAI&destination=DXB&date=2025-11-10&adults=1&currency=USD&max=20
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string origin,
                                                [FromQuery] string destination,
                                                [FromQuery(Name = "date")] DateTime departureDate,
                                                [FromQuery] DateTime? returnDate,
                                                [FromQuery] int adults = 1,
                                                [FromQuery] string? currency = null,
                                                [FromQuery] int? max = null)
        {
            var req = new FlightSearchRequestDto
            {
                Origin = origin?.Trim().ToUpperInvariant() ?? "",
                Destination = destination?.Trim().ToUpperInvariant() ?? "",
                DepartureDate = departureDate,
                ReturnDate = returnDate,
                Adults = adults,
                Currency = currency,
                Max = max
            };

            if (string.IsNullOrWhiteSpace(req.Origin) || string.IsNullOrWhiteSpace(req.Destination))
                return BadRequest(ApiResponse<string>.Fail("Origin and Destination are required (IATA codes)."));

            if (req.DepartureDate.Date < DateTime.UtcNow.Date)
                return BadRequest(ApiResponse<string>.Fail("Departure date must be today or later."));

            var results = await _amadeus.SearchAsync(req);
            return Ok(ApiResponse<IEnumerable<FlightOfferDto>>.Success(results, "Flight offers retrieved successfully"));
        }
    }
}
