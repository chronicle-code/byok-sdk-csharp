using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Byok.Models
{
    public class PlayerState
    {
        [JsonPropertyName("markers_activated")]
        public List<string> MarkersActivated { get; set; } = new List<string>();

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("custom")]
        public Dictionary<string, object> Custom { get; set; } = new Dictionary<string, object>();
    }

    public class UpdateStateRequest
    {
        /// <summary>Markers to add (merged additively).</summary>
        [JsonPropertyName("markers_activated")]
        public List<string>? MarkersActivated { get; set; }

        /// <summary>Current player location.</summary>
        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>Custom key-value data (deep-merged).</summary>
        [JsonPropertyName("custom")]
        public Dictionary<string, object>? Custom { get; set; }
    }
}
