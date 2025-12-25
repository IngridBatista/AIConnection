using AIConnection.Dtos.LLM;
using AIConnection.Services;
using AIConnection.Services.Claude;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace AIConnection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IChatClient _openAiClient;
        private readonly IChatClient _geminiClient;
        private readonly IChatClient _deepSeekClient;
        private readonly ClaudeService _claudeService;

        public AIController(
            [FromKeyedServices("openai")] IChatClient openAiClient,
            [FromKeyedServices("gemini")] IChatClient geminiClient,
            [FromKeyedServices("deepseek")] IChatClient deepSeekClient,
            ClaudeService claudeService)
        {
            _openAiClient = openAiClient;
            _geminiClient = geminiClient;
            _deepSeekClient = deepSeekClient;
            _claudeService = claudeService;
        }

        [HttpPost("geracao-codigo/openAI/gtp5")]
        public async Task<IActionResult> OpenAIGpt([FromBody] LargeLanguageModelRequest llmRequest)
        {
            try
            {
                var response = await _openAiClient.GetResponseAsync(llmRequest.Propmpt);

                ClassGenerationService.CreateClassFile(llmRequest, response.Text);

                return Ok(response.Text);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = "Service unavailable", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("geracao-codigo/claude/sonnet4.5")]
        public async Task<IActionResult> ClaudeSonnet([FromBody] LargeLanguageModelRequest llmRequest)
        {
            try
            {
                var response = await _claudeService.SendMessageWithSystemAsync(
                    userMessage: llmRequest.Propmpt,
                    systemPrompt: null,
                    model: "claude-sonnet-4-5-20250929",
                    temperature: null,
                    maxTokens: 1024
                );

                var textResponse = response.Content.FirstOrDefault()?.Text ?? string.Empty;

                ClassGenerationService.CreateClassFile(llmRequest, textResponse);

                return Ok(textResponse);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = "Service unavailable", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("geracao-codigo/google/gemini2.5")]
        public async Task<IActionResult> GoogleGemini([FromBody] LargeLanguageModelRequest llmRequest)
        {
            try
            {
                var response = await _geminiClient.GetResponseAsync(llmRequest.Propmpt);

                ClassGenerationService.CreateClassFile(llmRequest, response.Text);

                return Ok(response.Text);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = "Service unavailable", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("geracao-codigo/deepseek/deepseek-code")]
        public async Task<IActionResult> DeepSeekCode([FromBody] LargeLanguageModelRequest llmRequest)
        {
            try
            {
                var response = await _deepSeekClient.GetResponseAsync(llmRequest.Propmpt);

                ClassGenerationService.CreateClassFile(llmRequest, response.Text);

                return Ok(response.Text);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = "Service unavailable", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}
