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

        public AIController(IChatClient chatClient)
        {
            _chatClient = chatClient;
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

    }
}
