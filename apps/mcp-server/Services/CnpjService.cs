using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using McpServer.Models;

namespace McpServer.Services;

public class CnpjService(HttpClient httpClient) : ICnpjService
{
    private static readonly string[] SOURCES =
    [
        "https://brasilapi.com.br/api/cnpj/v1/",
        "https://minhareceita.org/",
    ];

    public async Task<string> FetchAsync(string cnpj, CancellationToken cancellationToken)
    {
        try
        {
            return await RequestAsync(cnpj, cancellationToken);
        }
        catch (Exception ex)
        {
            return $"Erro ao consultar o CNPJ {cnpj}: {ex.Message}";
        }
    }

    private async Task<string> RequestAsync(string cnpj, CancellationToken cancellationToken)
    {
        var company = await FetchFromSourcesAsync(cnpj, cancellationToken);
        if (company is null)
        {
            return $"Não foi possível obter os dados do CNPJ {cnpj} agora. Os serviços de consulta podem estar indisponíveis ou o CNPJ não existe. Tente novamente em instantes.";
        }

        return Serialize(company);
    }

    private async Task<CnpjApiResponse?> FetchFromSourcesAsync(string cnpj, CancellationToken cancellationToken)
    {
        foreach (var baseUrl in SOURCES)
        {
            var company = await TryFetchAsync($"{baseUrl}{cnpj}", cancellationToken);
            if (company is not null) return company;
        }

        return null;
    }

    private async Task<CnpjApiResponse?> TryFetchAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CnpjApiResponse>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static string Serialize(CnpjApiResponse company)
    {
        var result = new CnpjResult(
            company.RazaoSocial,
            company.SituacaoCadastral,
            company.AtividadePrincipal,
            company.Uf,
            company.Municipio);
        return JsonSerializer.Serialize(result);
    }

    private record CnpjApiResponse(
        [property: JsonPropertyName("razao_social")] string? RazaoSocial,
        [property: JsonPropertyName("descricao_situacao_cadastral")] string? SituacaoCadastral,
        [property: JsonPropertyName("cnae_fiscal_descricao")] string? AtividadePrincipal,
        [property: JsonPropertyName("uf")] string? Uf,
        [property: JsonPropertyName("municipio")] string? Municipio);
}
