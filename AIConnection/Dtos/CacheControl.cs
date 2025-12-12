using System.Text.Json.Serialization;

namespace AIConnection.Dtos
{
    public class CacheControl
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "ephemeral";
    }
}
