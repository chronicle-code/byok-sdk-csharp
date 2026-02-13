using System;
using System.Net.Http;

namespace Byok
{
    /// <summary>
    /// BYOK SDK client. Entry point for all API interactions.
    /// </summary>
    /// <example>
    /// <code>
    /// var client = new ByokClient("byok_sk_...");
    /// var response = await client.Chat.SendAsync(new ChatRequest { ... });
    /// </code>
    /// </example>
    public class ByokClient : IDisposable
    {
        private const string DefaultBaseUrl = "https://byok.gg";

        private readonly HttpClient _http;
        private readonly bool _ownsHttpClient;

        /// <summary>Chat completions (synchronous and streaming).</summary>
        public ChatApi Chat { get; }

        /// <summary>Player registration, profiles, highlights, spending, passport.</summary>
        public PlayersApi Players { get; }

        /// <summary>Game event polling, acknowledgement, and world event ingest.</summary>
        public EventsApi Events { get; }

        /// <summary>Per-player persistent state (markers, location, custom data).</summary>
        public StateApi State { get; }

        /// <summary>Player wallet balance and status.</summary>
        public WalletApi Wallet { get; }

        /// <summary>
        /// Create a client with just an API key (uses default base URL and a new HttpClient).
        /// </summary>
        public ByokClient(string apiKey, string? defaultUserId = null)
            : this(new ByokClientOptions { ApiKey = apiKey, DefaultUserId = defaultUserId })
        {
        }

        /// <summary>
        /// Create a client with full options.
        /// </summary>
        public ByokClient(ByokClientOptions options)
        {
            if (string.IsNullOrEmpty(options.ApiKey))
                throw new ArgumentException("ApiKey is required.", nameof(options));

            var baseUrl = (options.BaseUrl ?? DefaultBaseUrl).TrimEnd('/');

            if (options.HttpClient != null)
            {
                _http = options.HttpClient;
                _ownsHttpClient = false;
            }
            else
            {
                _http = new HttpClient();
                _ownsHttpClient = true;
            }

            Chat = new ChatApi(_http, baseUrl, options.ApiKey, options.DefaultUserId);
            Players = new PlayersApi(_http, baseUrl, options.ApiKey, options.DefaultUserId);
            Events = new EventsApi(_http, baseUrl, options.ApiKey, options.DefaultUserId);
            State = new StateApi(_http, baseUrl, options.ApiKey, options.DefaultUserId);
            Wallet = new WalletApi(_http, baseUrl, options.ApiKey, options.DefaultUserId);
        }

        public void Dispose()
        {
            if (_ownsHttpClient)
                _http.Dispose();
        }
    }

    /// <summary>Configuration options for ByokClient.</summary>
    public class ByokClientOptions
    {
        /// <summary>SDK API key (e.g. "byok_sk_...").</summary>
        public string ApiKey { get; set; } = "";

        /// <summary>Base URL of the BYOK API. Defaults to "https://byok.gg".</summary>
        public string? BaseUrl { get; set; }

        /// <summary>Default user ID sent via X-Byok-User header.</summary>
        public string? DefaultUserId { get; set; }

        /// <summary>
        /// Optional HttpClient instance. If provided, the client will not dispose it.
        /// If not provided, a new HttpClient is created and owned by the ByokClient.
        /// </summary>
        public HttpClient? HttpClient { get; set; }
    }
}
