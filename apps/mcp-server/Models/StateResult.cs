using System.Text.Json.Serialization;

namespace McpServer.Models;

public record StateResult(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("codigo")] int Codigo,
    [property: JsonPropertyName("sigla")] string Sigla,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("regiao")] string Regiao);
