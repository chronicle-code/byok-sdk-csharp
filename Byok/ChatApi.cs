using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Byok.Errors;
using Byok.Models;
using Byok.Streaming;

namespace Byok
{
    /// <summary>Chat completions -- synchronous and streaming.</summary>
    public class ChatApi
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string? _defaultUserId;

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        internal ChatApi(HttpClient http, string baseUrl, string apiKey, string? defaultUserId)
        {
            _http = http;
            _baseUrl = baseUrl;
            _apiKey = apiKey;
            _defaultUserId = defaultUserId;
        }

        /// <summary>Send a chat completion request and receive a full response.</summary>
        public async Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            var body = BuildBody(request, stream: false);
            using var httpReq = CreateRequest(request, body);
            var response = await _http.SendAsync(httpReq, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<ChatResponse>(json)
                   ?? throw new ByokException("Empty response body", (int)response.StatusCode);
        }

        /// <summary>Send a streaming chat request. Returns chunks as they arrive via SSE.</summary>
        public async IAsyncEnumerable<ChatStreamChunk> StreamAsync(
            ChatRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var body = BuildBody(request, stream: true);
            var httpReq = CreateRequest(request, body);
            HttpResponseMessage? response = null;
            try
            {
                response = await _http.SendAsync(httpReq, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    await ErrorParser.ThrowForStatusAsync(response).ConfigureAwait(false);

                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await foreach (var chunk in SseStreamReader.ReadAsync(stream, cancellationToken).ConfigureAwait(false))
                {
                    yield return chunk;
                }
            }
            finally
            {
                response?.Dispose();
                httpReq.Dispose();
            }
        }

        private HttpRequestMessage CreateRequest(ChatRequest request, string jsonBody)
        {
            var userId = request.UserId ?? _defaultUserId ?? "";
            var msg = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/v1/chat/completions")
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            msg.Headers.Add("Authorization", $"Bearer {_apiKey}");
            msg.Headers.Add("X-Byok-User", userId);
            if (request.SessionToken != null)
                msg.Headers.Add("X-Byok-Session", request.SessionToken);
            return msg;
        }

        private static string BuildBody(ChatRequest request, bool stream)
        {
            var dict = new Dictionary<string, object>
            {
                { "messages", request.Messages },
                { "stream", stream }
            };
            if (request.QualityTier.HasValue)
                dict["quality_tier"] = request.QualityTier.Value.ToString().ToLowerInvariant();
            if (request.LorevaultEntityRef != null)
                dict["lorevault_entity_ref"] = request.LorevaultEntityRef;
            if (request.GameState != null)
                dict["game_state"] = request.GameState;

            return JsonSerializer.Serialize(dict, JsonOpts);
        }
    }
}
