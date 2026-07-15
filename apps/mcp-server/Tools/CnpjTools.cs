using System.ComponentModel;
using McpServer.Services;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class CnpjTools
{
    [McpServerTool]
    [Description("Consulta dados cadastrais de uma empresa pelo CNPJ. Use quando o usuário informar um CNPJ ou pedir informações sobre uma empresa.")]
    public static Task<string> GetCnpjInfo(
        ICnpjService cnpjService,
        [Description("CNPJ com 14 dígitos numéricos, sem pontuação.")] string cnpj,
        CancellationToken cancellationToken)
    {
        if (!IsValid(cnpj))
        {
            return Task.FromResult("CNPJ inválido. Informe exatamente 14 dígitos numéricos, sem pontuação.");
        }

        return cnpjService.FetchAsync(cnpj, cancellationToken);
    }

    private static bool IsValid(string cnpj) =>
        cnpj is { Length: 14 } && cnpj.All(char.IsAsciiDigit);
}
