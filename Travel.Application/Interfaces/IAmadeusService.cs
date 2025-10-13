using Travel.Application.DTOs;

namespace Travel.Application.Interfaces
{
    public interface IAmadeusService
    {
        Task<IReadOnlyList<FlightOfferDto>> SearchAsync(
            FlightSearchRequestDto request,
            CancellationToken ct = default);


        Task<PricingResponseDto?> GetFlightPriceAsync(
            string offerId,
            CancellationToken ct = default);
    }

}
