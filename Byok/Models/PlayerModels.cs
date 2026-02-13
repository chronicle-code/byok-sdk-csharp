using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Byok.Models
{
    // ── Registration ──

    /// <summary>Base class for player registration requests.</summary>
    public abstract class RegisterPlayerRequest
    {
        /// <summary>Optional display name.</summary>
        public string? DisplayName { get; set; }

        internal abstract Dictionary<string, string> ToBody();
    }

    /// <summary>Register via Steam ID (e.g. "76561198...").</summary>
    public class SteamRegistration : RegisterPlayerRequest
    {
        public string SteamId { get; set; } = "";

        internal override Dictionary<string, string> ToBody()
        {
            var body = new Dictionary<string, string> { { "steam_id", SteamId } };
            if (DisplayName != null) body["display_name"] = DisplayName;
            return body;
        }
    }

    /// <summary>Register via anonymous device UUID.</summary>
    public class DeviceRegistration : RegisterPlayerRequest
    {
        public string DeviceId { get; set; } = "";

        internal override Dictionary<string, string> ToBody()
        {
            var body = new Dictionary<string, string> { { "device_id", DeviceId } };
            if (DisplayName != null) body["display_name"] = DisplayName;
            return body;
        }
    }

    /// <summary>Register via external auth provider (e.g. Epic, custom).</summary>
    public class ExternalRegistration : RegisterPlayerRequest
    {
        public string ExternalId { get; set; } = "";
        public string ExternalProvider { get; set; } = "";

        internal override Dictionary<string, string> ToBody()
        {
            var body = new Dictionary<string, string>
            {
                { "external_id", ExternalId },
                { "external_provider", ExternalProvider }
            };
            if (DisplayName != null) body["display_name"] = DisplayName;
            return body;
        }
    }

    public class RegisterPlayerResponse
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = "";

        [JsonPropertyName("created")]
        public bool Created { get; set; }
    }

    // ── Player Profile ──

    public class PlayerProfileDimensions
    {
        [JsonPropertyName("diplomacy_vs_aggression")]
        public double DiplomacyVsAggression { get; set; }

        [JsonPropertyName("curiosity")]
        public double Curiosity { get; set; }

        [JsonPropertyName("morality")]
        public double Morality { get; set; }
    }

    public class PlayerProfileResponse
    {
        [JsonPropertyName("player_id")]
        public string PlayerId { get; set; } = "";

        [JsonPropertyName("game_id")]
        public string GameId { get; set; } = "";

        [JsonPropertyName("dimensions")]
        public PlayerProfileDimensions Dimensions { get; set; } = new PlayerProfileDimensions();

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonPropertyName("total_interactions")]
        public int TotalInteractions { get; set; }

        [JsonPropertyName("social_investment")]
        public double SocialInvestment { get; set; }

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = "";
    }

    // ── Highlight Reel ──

    public class HighlightParams
    {
        /// <summary>Max number of highlights (server default: 50, max: 200).</summary>
        public int? Limit { get; set; }

        /// <summary>Only return highlights after this ISO 8601 timestamp.</summary>
        public string? Since { get; set; }
    }

    public class HighlightReelResponse
    {
        [JsonPropertyName("highlights")]
        public List<object> Highlights { get; set; } = new List<object>();
    }

    // ── Spending Intelligence ──

    public class SpendingIntelligenceResponse
    {
        /// <summary>Raw JSON data — use indexer or deserialize to a specific type.</summary>
        [JsonExtensionData]
        public Dictionary<string, object>? Data { get; set; }
    }

    // ── Passport ──

    public class PassportResponse
    {
        [JsonPropertyName("player_id")]
        public string PlayerId { get; set; } = "";

        [JsonPropertyName("profile")]
        public Dictionary<string, object> Profile { get; set; } = new Dictionary<string, object>();

        [JsonPropertyName("game_count")]
        public int GameCount { get; set; }

        [JsonPropertyName("total_interactions")]
        public int TotalInteractions { get; set; }

        [JsonPropertyName("last_computed_at")]
        public string LastComputedAt { get; set; } = "";
    }

    public class PassportConsentRequest
    {
        public bool Consent { get; set; }
    }

    public class PassportConsentResponse
    {
        [JsonPropertyName("consent_granted")]
        public bool ConsentGranted { get; set; }

        [JsonPropertyName("player_id")]
        public string PlayerId { get; set; } = "";
    }
}
