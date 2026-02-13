using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Byok.Errors;
using Byok.Models;

namespace Byok
{
    /// <summary>Game events: poll, acknowledge, batch-acknowledge, and ingest world events.</summary>
    public class EventsApi
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string? _defaultUserId;

        internal EventsApi(HttpClient http, string baseUrl, string apiKey, string? defaultUserId)
        {
            _http = http;
            _baseUrl = baseUrl;
            _apiKey = apiKey;
            _defaultUserId = defaultUserId;
        }

        /// <summary>
        /// Poll for unacknowledged game events.
        /// GET /api/v1/events
        /// </summary>
        public async Task<PollEventsResponse> PollAsync(PollEventsParams? parameters = null, string? userId = null, CancellationToken cancellationToken = default)
        {
            var url = $"{_baseUrl}/api/v1/events";
            if (parameters?.Since != null)
                url += $"?since={Uri.EscapeDataString(parameters.Since)}";

            using var msg = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<PollEventsResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Acknowledge a single event so it won't be returned again.
        /// POST /api/v1/events/:id/ack
        /// </summary>
        public async Task<AcknowledgeResponse> AckAsync(string eventId, string? userId = null, CancellationToken cancellationToken = default)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Post,
                $"{_baseUrl}/api/v1/events/{Uri.EscapeDataString(eventId)}/ack");
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<AcknowledgeResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Batch-acknowledge multiple events at once.
        /// POST /api/v1/events/ack
        /// </summary>
        public async Task<AcknowledgeBatchResponse> AckBatchAsync(List<string> eventIds, string? userId = null, CancellationToken cancellationToken = default)
        {
            var body = JsonSerializer.Serialize(new { event_ids = eventIds });
            using var msg = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/events/ack")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<AcknowledgeBatchResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>
        /// Ingest a game world event for deterministic processing (no AI cost).
        /// POST /api/v1/events/ingest
        /// </summary>
        public async Task<IngestEventResponse> IngestAsync(IngestEventRequest request, string? userId = null, CancellationToken cancellationToken = default)
        {
            var body = JsonSerializer.Serialize(new
            {
                event_type = request.EventType,
                payload = request.Payload
            });
            using var msg = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/events/ingest")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(msg, userId);

            var response = await _http.SendAsync(msg, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<IngestEventResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        private void AddAuthHeaders(HttpRequestMessage msg, string? userId)
        {
            msg.Headers.Add("Authorization", $"Bearer {_apiKey}");
            msg.Headers.Add("X-Byok-User", userId ?? _defaultUserId ?? "");
        }
    }
}
