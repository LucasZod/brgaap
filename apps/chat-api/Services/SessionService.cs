using System.Collections.Concurrent;
using Microsoft.Extensions.AI;

namespace ChatApi.Services;

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _sessions = new();

    public List<ChatMessage> GetOrCreate(string sessionId) =>
        _sessions.GetOrAdd(sessionId, _ => []);

    public void Append(string sessionId, ChatMessage message) =>
        GetOrCreate(sessionId).Add(message);
}
