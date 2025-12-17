using System.Text.Json.Serialization;

namespace AIConnection.Dtos.LLM.Claude
{
    public class SystemMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("cache_control")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CacheControl? CacheControl { get; set; }
    }
}
