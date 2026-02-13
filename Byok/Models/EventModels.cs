using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Byok.Models
{
    public class GameEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = "";

        [JsonPropertyName("payload")]
        public Dictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = "";
    }

    public class PollEventsParams
    {
        /// <summary>Only return events after this ISO 8601 timestamp.</summary>
        public string? Since { get; set; }
    }

    public class PollEventsResponse
    {
        [JsonPropertyName("events")]
        public List<GameEvent> Events { get; set; } = new List<GameEvent>();
    }

    public class AcknowledgeResponse
    {
        [JsonPropertyName("acknowledged")]
        public bool Acknowledged { get; set; }
    }

    public class AcknowledgeBatchResponse
    {
        [JsonPropertyName("acknowledged")]
        public int Acknowledged { get; set; }
    }

    public class IngestEventRequest
    {
        /// <summary>Event type (e.g. "world.kill", "world.discovery").</summary>
        public string EventType { get; set; } = "";

        /// <summary>Event-specific payload.</summary>
        public Dictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();
    }

    public class IngestEventResponse
    {
        [JsonPropertyName("processed")]
        public List<object> Processed { get; set; } = new List<object>();

        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = "";

        [JsonPropertyName("effects_count")]
        public int EffectsCount { get; set; }
    }
}
