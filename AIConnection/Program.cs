using AIConnection.Services.Claude;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

builder.Services.AddKeyedSingleton<IChatClient>("openai", (sp, key) =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? throw new InvalidOperationException("OPENAI_API_KEY not configured");

    var openAiClient = new OpenAI.Chat.ChatClient("gpt-5.1", apiKey).AsIChatClient();

    return openAiClient;
});

builder.Services.AddKeyedSingleton<IChatClient>("gemini", (sp, key) =>
{
    var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
                 ?? throw new InvalidOperationException("GOOGLE_API_KEY not configured");

    return new GeminiChatClient(new GeminiClientOptions
    {
        ApiKey = apiKey,
        ModelId = "gemini-2.0-flash-exp"
    });
});

builder.Services.AddHttpClient<ClaudeService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
