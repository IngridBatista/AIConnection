using System.Text.Json.Serialization;

namespace AIConnection.Dtos.LLM.Claude
{
    public class Usage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }

}
