namespace McpServer.Services;

public interface ICnpjService
{
    Task<string> FetchAsync(string cnpj, CancellationToken cancellationToken);
}
