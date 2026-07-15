using System.Net;
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
        using var response = await GetWithRetryAsync(cnpj, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return DescribeError(cnpj, response.StatusCode);
        }

        var company = await response.Content.ReadFromJsonAsync<BrasilApiCnpj>(cancellationToken);
        return company is null ? $"CNPJ {cnpj} sem dados retornados." : Serialize(company);
    }

    private async Task<HttpResponseMessage> GetWithRetryAsync(string cnpj, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"{BASE_URL}{cnpj}", cancellationToken);
        if (response.StatusCode != HttpStatusCode.TooManyRequests)
        {
            return response;
        }

        response.Dispose();
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        return await httpClient.GetAsync($"{BASE_URL}{cnpj}", cancellationToken);
    }

    private static string DescribeError(string cnpj, HttpStatusCode status)
    {
        if (status == HttpStatusCode.TooManyRequests)
        {
            return $"Limite de consultas da BrasilAPI atingido (HTTP 429) para o CNPJ {cnpj}. Aguarde alguns segundos e tente novamente.";
        }
        if (status == HttpStatusCode.NotFound)
        {
            return $"CNPJ {cnpj} não encontrado na base da BrasilAPI.";
        }
        return $"CNPJ {cnpj} indisponível no momento (HTTP {(int)status}).";
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
