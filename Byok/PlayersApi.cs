using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Byok.Errors;
using Byok.Models;

namespace Byok
{
    /// <summary>Player registration, profiles, highlights, spending, and passport.</summary>
    public class PlayersApi
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string? _defaultUserId;

        internal PlayersApi(HttpClient http, string baseUrl, string apiKey, string? defaultUserId)
        {
            _http = http;
            _baseUrl = baseUrl;
            _apiKey = apiKey;
            _defaultUserId = defaultUserId;
        }

        /// <summary>
        /// Register a player. Accepts SteamRegistration, DeviceRegistration, or ExternalRegistration.
        /// POST /api/v1/users/register
        /// </summary>
        public async Task<RegisterPlayerResponse> RegisterAsync(RegisterPlayerRequest request, CancellationToken cancellationToken = default)
        {
            var body = JsonSerializer.Serialize(request.ToBody());
            using var msg = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/users/register")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            msg.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<RegisterPlayerResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Get a player's behavioral profile.
        /// GET /api/v1/players/:id/profile
        /// </summary>
        public async Task<PlayerProfileResponse> GetProfileAsync(string playerId, string? userId = null, CancellationToken cancellationToken = default)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Get,
                $"{_baseUrl}/api/v1/players/{Uri.EscapeDataString(playerId)}/profile");
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<PlayerProfileResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Get a player's highlight reel.
        /// GET /api/v1/players/:id/highlights
        /// </summary>
        public async Task<HighlightReelResponse> GetHighlightsAsync(string playerId, HighlightParams? parameters = null, string? userId = null, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/v1/players/{Uri.EscapeDataString(playerId)}/highlights";
            var qs = BuildHighlightQuery(parameters);
            if (qs.Length > 0) url = url + "?" + qs;

            using var msg = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<HighlightReelResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Get a player's spending intelligence.
        /// GET /api/v1/players/:id/spending
        /// </summary>
        public async Task<SpendingIntelligenceResponse> GetSpendingAsync(string playerId, string? userId = null, CancellationToken cancellationToken = default)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Get,
                $"{_baseUrl}/api/v1/players/{Uri.EscapeDataString(playerId)}/spending");
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<SpendingIntelligenceResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Get a player's cross-game passport.
        /// GET /api/v1/passport/:user_id
        /// </summary>
        public async Task<PassportResponse> GetPassportAsync(string targetUserId, string? userId = null, CancellationToken cancellationToken = default)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Get,
                $"{_baseUrl}/api/v1/passport/{Uri.EscapeDataString(targetUserId)}");
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<PassportResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Grant or revoke cross-game passport sharing consent.
        /// POST /api/v1/passport/consent
        /// </summary>
        public async Task<PassportConsentResponse> SetPassportConsentAsync(PassportConsentRequest request, string? userId = null, CancellationToken cancellationToken = default)
        {
            var body = JsonSerializer.Serialize(new { consent = request.Consent });
            using var msg = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/passport/consent")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<PassportConsentResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        private void AddAuthHeaders(HttpRequestMessage msg, string? userId)
        {
            msg.Headers.Add("Authorization", $"Bearer {_apiKey}");
            msg.Headers.Add("X-Byok-User", userId ?? _defaultUserId ?? "");
        }

        private static string BuildHighlightQuery(HighlightParams? p)
        {
            if (p == null) return "";
            var parts = new System.Collections.Generic.List<string>();
            if (p.Limit.HasValue) parts.Add($"limit={p.Limit.Value}");
            if (p.Since != null) parts.Add($"since={Uri.EscapeDataString(p.Since)}");
            return string.Join("&", parts);
        }
    }
}
