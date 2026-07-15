using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using McpServer.Models;

namespace McpServer.Services;

public class CnpjService(HttpClient httpClient) : ICnpjService
{
    private const string BASE_URL = "https://brasilapi.com.br/api/cnpj/v1/";

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
        using var response = await httpClient.GetAsync($"{BASE_URL}{cnpj}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return $"CNPJ {cnpj} não encontrado ou indisponível (HTTP {(int)response.StatusCode}).";
        }

        var company = await response.Content.ReadFromJsonAsync<BrasilApiCnpj>(cancellationToken);
        return company is null ? $"CNPJ {cnpj} sem dados retornados." : Serialize(company);
    }

    private static string Serialize(BrasilApiCnpj company)
    {
        var result = new CnpjResult(
            company.RazaoSocial,
            company.SituacaoCadastral,
            company.AtividadePrincipal,
            company.Uf,
            company.Municipio);
        return JsonSerializer.Serialize(result);
    }

    private record BrasilApiCnpj(
        [property: JsonPropertyName("razao_social")] string? RazaoSocial,
        [property: JsonPropertyName("descricao_situacao_cadastral")] string? SituacaoCadastral,
        [property: JsonPropertyName("cnae_fiscal_descricao")] string? AtividadePrincipal,
        [property: JsonPropertyName("uf")] string? Uf,
        [property: JsonPropertyName("municipio")] string? Municipio);
}
