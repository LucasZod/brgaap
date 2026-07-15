using ChatApi.Endpoints;
using ChatApi.Services;
using Microsoft.Extensions.AI;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins("http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddSingleton<IChatClient>(_ => BuildChatClient(builder.Configuration));
builder.Services.AddSingleton<IMcpClientService, McpClientService>();
builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddSingleton<IChatService, ChatService>();

var app = builder.Build();

app.UseCors();

await app.Services.GetRequiredService<IMcpClientService>().InitializeAsync();

ChatEndpoints.Map(app);

await app.RunAsync();

static IChatClient BuildChatClient(IConfiguration configuration)
{
    var url = configuration["Ollama:Url"] ?? "http://localhost:11434";
    var model = configuration["Ollama:Model"] ?? "llama3.2";
    var httpClient = new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromMinutes(10) };
    return new ChatClientBuilder(new OllamaApiClient(httpClient, model))
        .UseFunctionInvocation()
        .Build();
}
