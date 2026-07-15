using Microsoft.Extensions.AI;

namespace ChatApi.Services;

public interface ISessionService
{
    List<ChatMessage> GetOrCreate(string sessionId);

    void Append(string sessionId, ChatMessage message);
}
