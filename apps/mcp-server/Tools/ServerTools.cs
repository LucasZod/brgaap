using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class ServerTools
{
    [McpServerTool]
    [Description("Retorna a data e hora atual do servidor. Use quando o usuário perguntar que horas são ou qual a data de hoje.")]
    public static string GetServerDateTime() =>
        DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
}
