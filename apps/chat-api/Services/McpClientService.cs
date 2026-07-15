using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ChatApi.Services;

public class McpClientService : IMcpClientService
{
    private readonly string _serverProjectPath;
    private McpClient? _client;

    public IReadOnlyList<AITool> Tools { get; private set; } = [];

    public McpClientService(IConfiguration configuration)
    {
        var relativePath = configuration["Mcp:ServerProject"] ?? "../mcp-server";
        _serverProjectPath = ResolveProjectPath(relativePath);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _client = await McpClient.CreateAsync(CreateTransport(), cancellationToken: cancellationToken);
        var tools = await _client.ListToolsAsync(cancellationToken: cancellationToken);
        Tools = [.. tools];
    }

    private StdioClientTransport CreateTransport() =>
        new(new StdioClientTransportOptions
        {
            Name = "mcp-server",
            Command = "dotnet",
            Arguments = ["run", "--project", _serverProjectPath, "--no-build"]
        });

    private static string ResolveProjectPath(string relativePath)
    {
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));
        return Path.GetFullPath(Path.Combine(projectDir, relativePath));
    }
}
