using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using McpServer.Models;

namespace McpServer.Services;

public class IbgeService(HttpClient httpClient) : IIbgeService
{
    private const string BASE_URL = "https://servicodados.ibge.gov.br/api/v1/localidades/estados";

    public async Task<string> GetStatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await FetchStatesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return $"Erro ao consultar os estados: {ex.Message}";
        }
    }

    public async Task<string> GetMunicipalitiesAsync(string uf, CancellationToken cancellationToken)
    {
        try
        {
            return await FetchMunicipalitiesAsync(uf, cancellationToken);
        }
        catch (Exception ex)
        {
            return $"Erro ao consultar os municípios de {uf}: {ex.Message}";
        }
    }

    private async Task<string> FetchStatesAsync(CancellationToken cancellationToken)
    {
        var url = $"{BASE_URL}?orderBy=nome";
        var states = await httpClient.GetFromJsonAsync<IbgeState[]>(url, cancellationToken);
        if (states is null)
        {
            return "Nenhum estado retornado pela API do IBGE.";
        }

        var result = states.Select(state => state.ToResult());
        return JsonSerializer.Serialize(result);
    }

    private async Task<string> FetchMunicipalitiesAsync(string uf, CancellationToken cancellationToken)
    {
        var url = $"{BASE_URL}/{uf}/municipios?orderBy=nome";
        var municipalities = await httpClient.GetFromJsonAsync<MunicipalityResult[]>(url, cancellationToken);
        if (municipalities is null)
        {
            return $"Nenhum município retornado para o estado {uf}.";
        }

        return JsonSerializer.Serialize(municipalities);
    }

    private record IbgeState(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("sigla")] string Sigla,
        [property: JsonPropertyName("nome")] string Nome,
        [property: JsonPropertyName("regiao")] IbgeRegion? Regiao)
    {
        public StateResult ToResult() => new(Id, Codigo: Id, Sigla, Nome, Regiao?.Nome ?? "");
    }

    private record IbgeRegion([property: JsonPropertyName("nome")] string Nome);
}
