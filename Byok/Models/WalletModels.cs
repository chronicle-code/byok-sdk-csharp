using System.Text.Json.Serialization;

namespace Byok.Models
{
    public class WalletResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("balance_cents")]
        public int BalanceCents { get; set; }

        [JsonPropertyName("held_cents")]
        public int HeldCents { get; set; }

        [JsonPropertyName("available_cents")]
        public int AvailableCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("quality_preference")]
        public string? QualityPreference { get; set; }
    }
}
