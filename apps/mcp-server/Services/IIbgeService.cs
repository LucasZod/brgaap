namespace McpServer.Services;

public interface IIbgeService
{
    Task<string> GetStatesAsync(CancellationToken cancellationToken);

    Task<string> GetMunicipalitiesAsync(string uf, CancellationToken cancellationToken);
}
