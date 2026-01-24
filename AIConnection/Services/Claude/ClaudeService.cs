using AIConnection.Dtos.LLM.Claude;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIConnection.Services.Claude;

public class ClaudeService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public ClaudeService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var apiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
            ?? throw new InvalidOperationException("Anthropic API key not configured");

        if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

        if (!_httpClient.DefaultRequestHeaders.Contains("anthropic-version"))
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<ClaudeResponse> SendMessageWithSystemAsync(
        string userMessage,
        string? systemPrompt = null,
        string model = "claude-sonnet-4-5-20250929",
        double? temperature = null,
        int maxTokens = 6000,
        CancellationToken cancellationToken = default)
    {
        var request = new ClaudeRequest
        {
            Model = model,
            MaxTokens = maxTokens,
            Temperature = temperature,
            Messages = new()
            {
                new Message
                {
                    Role = "user",
                    Content = userMessage
                }
            },
            System = string.IsNullOrWhiteSpace(systemPrompt)
                ? null
                : new List<SystemMessage>
                {
                    new SystemMessage { Type = "text", Text = systemPrompt }
                }
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<ClaudeResponse>(responseJson)
            ?? throw new InvalidOperationException("Failed to deserialize Claude response");
    }
}