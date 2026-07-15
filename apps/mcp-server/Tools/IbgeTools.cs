using System.ComponentModel;
using McpServer.Services;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class IbgeTools
{
    [McpServerTool]
    [Description("Retorna TODOS os estados brasileiros (UF) com código IBGE, sigla, nome e região. Use para qualquer pergunta sobre um ou mais estados — incluindo a sigla, o código IBGE, o nome ou a região de um estado específico como Goiás, São Paulo ou Bahia.")]
    public static Task<string> GetStates(
        IIbgeService ibgeService,
        CancellationToken cancellationToken) =>
        ibgeService.GetStatesAsync(cancellationToken);

    [McpServerTool]
    [Description("Retorna a lista de cidades (municípios) de UM estado. Use somente quando o usuário pedir as cidades ou municípios de um estado. NÃO use para obter a sigla, o código IBGE ou dados do próprio estado — para isso use a ferramenta de estados.")]
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
