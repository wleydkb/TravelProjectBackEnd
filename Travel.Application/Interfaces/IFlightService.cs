using Travel.Core.Entities;

namespace Travel.Application.Interfaces
{
    public interface IFlightService
    {
        Task<IEnumerable<FlightCache>> SearchFlightsAsync(string origin, string destination, DateTime departureDate);
        Task<FlightCache?> GetFlightByOfferIdAsync(string offerId);
        Task AddFlightToCacheAsync(FlightCache flight);
    }
}
