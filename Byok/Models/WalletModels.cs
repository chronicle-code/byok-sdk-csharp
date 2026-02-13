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

    public class CheckoutRequest
    {
        /// <summary>Amount in cents (100-50000). e.g. 1000 = $10.00</summary>
        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        /// <summary>Custom success redirect URL. Defaults to BYOK's hosted success page.</summary>
        [JsonPropertyName("success_url")]
        public string? SuccessUrl { get; set; }

        /// <summary>Custom cancel redirect URL. Defaults to BYOK's hosted cancel page.</summary>
        [JsonPropertyName("cancel_url")]
        public string? CancelUrl { get; set; }
    }

    public class CheckoutResponse
    {
        /// <summary>Stripe Checkout session URL. Open this in a browser/webview for the player to complete payment.</summary>
        [JsonPropertyName("checkout_url")]
        public string CheckoutUrl { get; set; } = "";
    }
}
