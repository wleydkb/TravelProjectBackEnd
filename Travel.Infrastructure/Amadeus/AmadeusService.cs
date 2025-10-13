using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Travel.Application.DTOs;
using Travel.Application.Interfaces;
using Travel.Core.Entities;
using Travel.Infrastructure.Data;

namespace Travel.Infrastructure.Amadeus
{
    public class AmadeusService : IAmadeusService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAmadeusAuthService _auth;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;

        public AmadeusService(
            IHttpClientFactory httpClientFactory,
            IAmadeusAuthService auth,
            IConfiguration config,
            AppDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _auth = auth;
            _config = config;
            _db = db;
        }

        public async Task<IReadOnlyList<FlightOfferDto>> SearchAsync(FlightSearchRequestDto req, CancellationToken ct = default)
        {
            var ttlMinutes = int.TryParse(_config["Amadeus:CacheTtlMinutes"], out var ttl) ? ttl : 15;
            var cutoff = DateTime.UtcNow.AddMinutes(-ttlMinutes);

            // 1) جرّب الكاش: نفس (origin, destination, departure date)
            var depDateOnly = req.DepartureDate.Date;
            var cached = await _db.FlightCaches
                .AsNoTracking()
                .Where(x =>
                       x.Origin == req.Origin &&
                       x.Destination == req.Destination &&
                       x.DepartureDate.Date == depDateOnly &&
                       x.CachedAt >= cutoff)
                .OrderByDescending(x => x.CachedAt)
                .ToListAsync(ct);

            if (cached.Any())
                return cached.Select(MapFromCache).ToList();

            // 2) نداء Amadeus
            var client = _httpClientFactory.CreateClient("Amadeus");
            var token = await _auth.GetTokenAsync(ct);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var query = new Dictionary<string, string?>
            {
                ["originLocationCode"] = req.Origin,
                ["destinationLocationCode"] = req.Destination,
                ["departureDate"] = req.DepartureDate.ToString("yyyy-MM-dd"),
                ["adults"] = Math.Max(1, req.Adults).ToString(),
                ["currencyCode"] = req.Currency ?? _config["Amadeus:DefaultCurrency"] ?? "USD",
                ["max"] = (req.Max ?? int.Parse(_config["Amadeus:MaxResults"] ?? "20")).ToString()
            };
            if (req.ReturnDate.HasValue)
                query["returnDate"] = req.ReturnDate.Value.ToString("yyyy-MM-dd");

            var qs = string.Join("&", query.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                                           .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));

            using var resp = await client.GetAsync($"/v2/shopping/flight-offers?{qs}", ct);
            resp.EnsureSuccessStatusCode();

            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            var data = doc.RootElement.GetProperty("data");

            var offers = new List<FlightOfferDto>();
            using var tx = await _db.Database.BeginTransactionAsync(ct);

            foreach (var offerEl in data.EnumerateArray())
            {
                var id = offerEl.GetProperty("id").GetString() ?? Guid.NewGuid().ToString("N");

                // أول segment
                var firstSeg = offerEl.GetProperty("itineraries")[0]
                                      .GetProperty("segments")[0];

                var depAt = DateTime.Parse(firstSeg.GetProperty("departure").GetProperty("at").GetString()!);
                var arrAt = DateTime.Parse(firstSeg.GetProperty("arrival").GetProperty("at").GetString()!);
                var airline = firstSeg.GetProperty("carrierCode").GetString() ?? "";

                // السعر
                var priceObj = offerEl.GetProperty("price");
                var total = decimal.Parse(priceObj.GetProperty("total").GetString()!, System.Globalization.CultureInfo.InvariantCulture);
                var curr = priceObj.GetProperty("currency").GetString() ?? "USD";

                // مدة ورحلات
                var duration = offerEl.GetProperty("itineraries")[0].GetProperty("duration").GetString() ?? "";
                var stops = offerEl.GetProperty("itineraries")[0].GetProperty("segments").GetArrayLength() - 1;

                offers.Add(new FlightOfferDto
                {
                    OfferId = id,
                    Origin = req.Origin,
                    Destination = req.Destination,
                    DepartureAt = depAt,
                    ArrivalAt = arrAt,
                    Airline = airline,
                    Duration = duration,
                    Stops = stops,
                    Price = total,
                    Currency = curr
                });

                // خزّن كل عرض كصف في الكاش
                _db.FlightCaches.Add(new FlightCache
                {
                    OfferId = id,
                    Origin = req.Origin,
                    Destination = req.Destination,
                    DepartureDate = depDateOnly,
                    Airline = airline,
                    Price = total,
                    Currency = curr,
                    RawResponse = offerEl.GetRawText(),
                    CachedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return offers;
        }



        public async Task<PricingResponseDto?> GetFlightPriceAsync(string offerId, CancellationToken ct = default)
        {
            var cache = await _db.FlightCaches.FirstOrDefaultAsync(x => x.OfferId == offerId, ct);
            if (cache == null)
                return null;

            var client = _httpClientFactory.CreateClient("Amadeus");
            var token = await _auth.GetTokenAsync(ct);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // نستخدم الـ raw response اللي خزنّاه من البحث
            var json = cache.RawResponse;

            var body = new
            {
                data = new
                {
                    type = "flight-offers-pricing",
                    flightOffers = new[]
                    {
                System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json)
            }
                }
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json");

            var resp = await client.PostAsync("/v1/shopping/flight-offers/pricing", content, ct);
            resp.EnsureSuccessStatusCode();

            var doc = System.Text.Json.JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            var price = doc.RootElement.GetProperty("data").GetProperty("flightOffers")[0].GetProperty("price");
            var total = decimal.Parse(price.GetProperty("total").GetString()!, System.Globalization.CultureInfo.InvariantCulture);
            var currency = price.GetProperty("currency").GetString() ?? "USD";

            return new PricingResponseDto
            {
                OfferId = offerId,
                Total = total,
                Currency = currency,
                Airline = cache.Airline,
                Origin = cache.Origin,
                Destination = cache.Destination,
                DepartureAt = cache.DepartureDate,
                ArrivalAt = cache.DepartureDate
            };
        }


        private static FlightOfferDto MapFromCache(FlightCache c) => new()
        {
            OfferId = c.OfferId,
            Origin = c.Origin,
            Destination = c.Destination,
            DepartureAt = c.DepartureDate,     // best-effort من الكاش
            ArrivalAt = c.DepartureDate,     // غير متاح بالكاش المختصر—ممكن نقرأها من RawResponse لو حبيت
            Airline = c.Airline,
            Duration = "",
            Stops = 0,
            Price = c.Price,
            Currency = c.Currency
        };
    }
}
