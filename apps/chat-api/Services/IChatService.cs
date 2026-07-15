namespace ChatApi.Services;

public interface IChatService
{
    IAsyncEnumerable<string> StreamAsync(string sessionId, string message, CancellationToken cancellationToken);
}
