using System.ComponentModel;
using McpServer.Services;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class IbgeTools
{
    [McpServerTool]
    [Description("Retorna a lista de todos os estados brasileiros com código IBGE, sigla e nome.")]
    public static Task<string> GetStates(
        IIbgeService ibgeService,
        CancellationToken cancellationToken) =>
        ibgeService.GetStatesAsync(cancellationToken);

    [McpServerTool]
    [Description("Retorna a lista de municípios de um estado brasileiro. Requer a sigla do estado (UF).")]
    public static Task<string> GetMunicipalities(
        IIbgeService ibgeService,
        [Description("Sigla do estado (UF), ex: GO, SP, RJ.")] string uf,
        CancellationToken cancellationToken)
    {
        var normalized = uf?.Trim().ToUpperInvariant() ?? "";
        if (!IsValidUf(normalized))
        {
            return Task.FromResult("UF inválida. Informe a sigla do estado com 2 letras, ex: GO, SP, RJ.");
        }

        return ibgeService.GetMunicipalitiesAsync(normalized, cancellationToken);
    }

    private static bool IsValidUf(string uf) =>
        uf is { Length: 2 } && uf.All(char.IsAsciiLetter);
}
