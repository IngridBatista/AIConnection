using AIConnection.Dtos;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIConnection.Services;

public class ClaudeService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public ClaudeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
            ?? throw new InvalidOperationException("Anthropic API key not configured");

        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<ClaudeResponse> SendMessageWithSystemAsync(
        string userMessage,
        string? systemPrompt = null,
        string model = "claude-sonnet-4-5-20250929",
        double? temperature = null,
        int maxTokens = 1024)
    {
        var request = new ClaudeRequest
        {
            Model = model,
            MaxTokens = maxTokens,
            Temperature = temperature,
            Messages = new List<Message>
            {
                new Message { Role = "user", Content = userMessage }
            }
        };

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            request.System = new List<SystemMessage>
            {
                new SystemMessage { Type = "text", Text = systemPrompt }
            };
        }

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiUrl, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseJson);

        return claudeResponse ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}