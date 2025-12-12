using AIConnection.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

builder.Services.AddChatClient(services =>
    new OpenAI.Chat.ChatClient("gpt-5.1", apiKey).AsIChatClient())
    .UseDistributedCache(cache);

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
