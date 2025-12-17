using System.Text.Json.Serialization;

namespace AIConnection.Dtos.LLM.Claude
{
    public class CacheControl
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "ephemeral";
    }
}
