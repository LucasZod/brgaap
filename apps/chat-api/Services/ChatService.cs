using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.AI;

namespace ChatApi.Services;

public class ChatService(
    IChatClient chatClient,
    ISessionService sessionService,
    IMcpClientService mcpClientService) : IChatService
{
    private const string SYSTEM_PROMPT =
        "Você é um assistente especializado em gestão pública brasileira. " +
        "Você tem acesso a ferramentas para consultar CNPJs de empresas, " +
        "estados e municípios brasileiros via IBGE, e a data/hora do servidor. " +
        "Use as ferramentas sempre que o usuário pedir dados que elas podem fornecer. " +
        "Ao receber o resultado de uma ferramenta, apresente os dados reais ao usuário " +
        "de forma clara e organizada: quando houver vários itens, liste-os em bullets. " +
        "Nunca responda apenas que 'a resposta é uma lista' sem mostrar os itens. " +
        "Responda sempre em português brasileiro.";

    public async IAsyncEnumerable<string> StreamAsync(
        string sessionId,
        string message,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var history = PrepareHistory(sessionId, message);
        var options = new ChatOptions { Tools = [.. mcpClientService.Tools] };
        var answer = new StringBuilder();

        await foreach (var update in chatClient.GetStreamingResponseAsync(history, options, cancellationToken))
        {
            if (string.IsNullOrEmpty(update.Text))
            {
                continue;
            }

            answer.Append(update.Text);
            yield return update.Text;
        }

        sessionService.Append(sessionId, new ChatMessage(ChatRole.Assistant, answer.ToString()));
    }

    private List<ChatMessage> PrepareHistory(string sessionId, string message)
    {
        var history = sessionService.GetOrCreate(sessionId);
        if (history.Count == 0)
        {
            sessionService.Append(sessionId, new ChatMessage(ChatRole.System, SYSTEM_PROMPT));
        }

        sessionService.Append(sessionId, new ChatMessage(ChatRole.User, message));
        return history;
    }
}
