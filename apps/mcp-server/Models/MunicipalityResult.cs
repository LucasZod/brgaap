using System.Text.Json.Serialization;

namespace McpServer.Models;

public record MunicipalityResult(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Nome);
