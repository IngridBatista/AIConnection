using System.Text.Json.Serialization;

namespace AIConnection.Dtos.LLM.Claude
{
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
