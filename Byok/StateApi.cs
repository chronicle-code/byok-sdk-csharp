using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Byok.Errors;
using Byok.Models;

namespace Byok
{
    /// <summary>Player state: markers, location, and custom persistent data.</summary>
    public class StateApi
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string? _defaultUserId;

        internal StateApi(HttpClient http, string baseUrl, string apiKey, string? defaultUserId)
        {
            _http = http;
            _baseUrl = baseUrl;
            _apiKey = apiKey;
            _defaultUserId = defaultUserId;
        }

        /// <summary>
        /// Get the current player state.
        /// GET /api/v1/state
        /// </summary>
        public async Task<PlayerState> GetAsync(string? userId = null, CancellationToken cancellationToken = default)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/v1/state");
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<PlayerState>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Update player state. Markers are merged additively, custom data is deep-merged.
        /// POST /api/v1/state
        /// </summary>
        public async Task<PlayerState> UpdateAsync(UpdateStateRequest request, string? userId = null, CancellationToken cancellationToken = default)
        {
            var opts = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var body = JsonSerializer.Serialize(request, opts);
            using var msg = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/state")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<PlayerState>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        private void AddAuthHeaders(HttpRequestMessage msg, string? userId)
        {
            msg.Headers.Add("Authorization", $"Bearer {_apiKey}");
            msg.Headers.Add("X-Byok-User", userId ?? _defaultUserId ?? "");
        }
    }
}
