using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Byok.Models
{
    public enum QualityTier
    {
        Budget,
        Standard,
        Ultra
    }

    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }

    public class ChatRequest
    {
        /// <summary>Message history. Typically a single user message or a conversation array.</summary>
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        /// <summary>AI quality tier. Defaults to Standard.</summary>
        public QualityTier? QualityTier { get; set; }

        /// <summary>Enable SSE streaming response.</summary>
        public bool Stream { get; set; }

        /// <summary>LoreVault entity reference, e.g. "characters/blacksmith-thorn".</summary>
        public string? LorevaultEntityRef { get; set; }

        /// <summary>Arbitrary game state passed as context to the AI.</summary>
        public Dictionary<string, object>? GameState { get; set; }

        /// <summary>Player/user ID. Overrides the client-level DefaultUserId for this request.</summary>
        public string? UserId { get; set; }

        /// <summary>Worker session token (for LoreVault worker sessions).</summary>
        public string? SessionToken { get; set; }
    }

    public class ChatChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public ChatMessage Message { get; set; } = new ChatMessage();

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = "";
    }

    public class ChatUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class ChatResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("object")]
        public string Object { get; set; } = "";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("choices")]
        public List<ChatChoice> Choices { get; set; } = new List<ChatChoice>();

        [JsonPropertyName("usage")]
        public ChatUsage Usage { get; set; } = new ChatUsage();
    }

    public class ChatStreamChunk
    {
        /// <summary>The text content delta for this chunk.</summary>
        public string Content { get; set; } = "";

        /// <summary>True on the final chunk.</summary>
        public bool Done { get; set; }
    }
}
