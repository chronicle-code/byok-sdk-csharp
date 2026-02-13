using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Byok.Errors;
using Byok.Models;

namespace Byok
{
    /// <summary>Wallet endpoint: check player balance and status.</summary>
    public class WalletApi
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string? _defaultUserId;

        internal WalletApi(HttpClient http, string baseUrl, string apiKey, string? defaultUserId)
        {
            _http = http;
            _baseUrl = baseUrl;
            _apiKey = apiKey;
            _defaultUserId = defaultUserId;
        }

        /// <summary>
        /// Get the current user's wallet info.
        /// GET /api/v1/wallet
        /// </summary>
        public async Task<WalletResponse> GetAsync(string? userId = null, CancellationToken cancellationToken = default)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/v1/wallet");
            msg.Headers.Add("Authorization", $"Bearer {_apiKey}");
            msg.Headers.Add("X-Byok-User", userId ?? _defaultUserId ?? "");

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<WalletResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Create a Stripe Checkout session for wallet top-up.
        /// Returns a URL to open in a browser/webview for the player to complete payment.
        /// POST /api/v1/wallet/checkout
        /// </summary>
        public async Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request, string? userId = null, CancellationToken cancellationToken = default)
        {
            var body = JsonSerializer.Serialize(request);
            using var msg = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/wallet/checkout");
            msg.Headers.Add("Authorization", $"Bearer {_apiKey}");
            msg.Headers.Add("X-Byok-User", userId ?? _defaultUserId ?? "");
            msg.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<CheckoutResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }
    }
}
