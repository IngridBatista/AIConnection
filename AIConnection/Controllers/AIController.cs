using AIConnection.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using System.Text.RegularExpressions;

namespace AIConnection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IChatClient _chatClient;
        private readonly ClaudeService _claudeService;

        public AIController(IChatClient chatClient, ClaudeService claudeService)
        {
            _chatClient = chatClient;
            _claudeService = claudeService;
        }

        [HttpPost("geracao-codigo/openAI/gtp5")]
        public async Task<IActionResult> OpenAIGpt5([FromBody] string question)
        {
            var response = await _chatClient.GetResponseAsync(question);

            var match = Regex.Match(response.Text, @"```csharp([\s\S]*?)```");

            if (!match.Success)
            {
                Console.WriteLine("Bloco csharp não encontrado.");
            }

            string code = match.Groups[1].Value.Trim();

            var classMatch = Regex.Match(code, @"class\s+([A-Za-z_][A-Za-z0-9_]*)");

            if (!classMatch.Success)
            {
                Console.WriteLine("Nenhuma classe encontrada no código.");
            }

            string className = classMatch.Groups[1].Value;
            string fileName = className + ".cs";

            System.IO.File.WriteAllText(fileName, code);

            return Ok(response.Text);
        }

        [HttpPost("geracao-codigo/claude/sonnet4.5")]
        public async Task<IActionResult> SendMessage([FromBody] string question)
        {
            try
            {
                var response = await _claudeService.SendMessageWithSystemAsync(
                    userMessage: question,
                    systemPrompt: null,
                    model: "claude-sonnet-4-5-20250929",
                    temperature: null,
                    maxTokens: 1024
                );

                var textResponse = response.Content.FirstOrDefault()?.Text ?? string.Empty;

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
    }
}
