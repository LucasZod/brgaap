using System.Text.Json.Serialization;

namespace McpServer.Models;

public record CnpjResult(
    [property: JsonPropertyName("razaoSocial")] string? RazaoSocial,
    [property: JsonPropertyName("situacaoCadastral")] string? SituacaoCadastral,
    [property: JsonPropertyName("atividadePrincipal")] string? AtividadePrincipal,
    [property: JsonPropertyName("uf")] string? Uf,
    [property: JsonPropertyName("municipio")] string? Municipio);
