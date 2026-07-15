using ChatApi.Models;
using ChatApi.Services;

namespace ChatApi.Endpoints;

public static class ChatEndpoints
{
    public static void Map(WebApplication app) =>
        app.MapPost("/chat", HandleAsync);

    private static async Task HandleAsync(
        ChatRequest request,
        IChatService chatService,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        http.Response.Headers.ContentType = "text/event-stream";
        http.Response.Headers.CacheControl = "no-cache";

        try
        {
            await StreamDeltasAsync(request, chatService, http, cancellationToken);
            await SendAsync(http, "data: [DONE]\n\n", cancellationToken);
        }
        catch (Exception ex)
        {
            await SendAsync(http, $"data: [ERROR] {ex.Message}\n\n", cancellationToken);
        }
    }

    private static async Task StreamDeltasAsync(
        ChatRequest request,
        IChatService chatService,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var stream = chatService.StreamAsync(request.SessionId, request.Message, cancellationToken);
        await foreach (var delta in stream)
        {
            await SendAsync(http, $"data: {delta}\n\n", cancellationToken);
        }
    }

    private static async Task SendAsync(HttpContext http, string payload, CancellationToken cancellationToken)
    {
        await http.Response.WriteAsync(payload, cancellationToken);
        await http.Response.Body.FlushAsync(cancellationToken);
    }
}
