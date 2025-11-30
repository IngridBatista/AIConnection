using Microsoft.AspNetCore.Mvc;
using OpenAI;

namespace AIConnection.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AIController : ControllerBase
    {
        private readonly OpenAIClient _client;

        public AIController(OpenAIClient client)
        {
            _client = client;
        }


        [HttpGet("perguntar")]
        public async Task<string> Get(string pergunta)
        {
            var response = await _client.Chat.Completions.CreateAsync(
                model: "gpt-5",
                messages: new[] { new ChatMessage("user", pergunta) }
            );

            return response.Choices[0].Message.Content[0].Text;
        }
    }
}
