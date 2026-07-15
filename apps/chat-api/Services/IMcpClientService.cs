using Microsoft.Extensions.AI;

namespace ChatApi.Services;

public interface IMcpClientService
{
    IReadOnlyList<AITool> Tools { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);
}
