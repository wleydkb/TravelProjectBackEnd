using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using Travel.Application.Interfaces;

namespace Travel.Infrastructure.Amadeus
{
    public class AmadeusAuthService : IAmadeusAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "AMADEUS_TOKEN";

        public AmadeusAuthService(IHttpClientFactory httpClientFactory, IConfiguration config, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _cache = cache;
        }

        public async Task<string> GetTokenAsync(CancellationToken ct = default)
        {
            if (_cache.TryGetValue(CacheKey, out string token) && !string.IsNullOrEmpty(token))
                return token;

            var client = _httpClientFactory.CreateClient("Amadeus");

            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _config["Amadeus:ClientId"]!,
                ["client_secret"] = _config["Amadeus:ClientSecret"]!
            });

            using var resp = await client.PostAsync("/v1/security/oauth2/token", form, ct);
            resp.EnsureSuccessStatusCode();

            var payload = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
            if (payload is null || string.IsNullOrEmpty(payload.access_token))
                throw new InvalidOperationException("Failed to obtain Amadeus access token.");


            var ttl = TimeSpan.FromSeconds(Math.Max(60, payload.expires_in - 60));
            _cache.Set(CacheKey, payload.access_token, ttl);

            return payload.access_token;
        }

        private sealed class TokenResponse
        {
            public string token_type { get; set; } = "";
            public string access_token { get; set; } = "";
            public int expires_in { get; set; }
            public string? scope { get; set; }
        }
    }
}