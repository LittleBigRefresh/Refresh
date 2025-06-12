#if POSTGRES

using Testcontainers.PostgreSql;

namespace RefreshTests.GameServer.GameServer;

public class ContainerPool : IDisposable
{
    public static readonly ContainerPool Instance = new();

    private readonly Queue<PostgreSqlContainer> _containers = [];
    private readonly List<PostgreSqlContainer> _allContainers = [];

    public PostgreSqlContainer Take()
    {
        // PostgreSqlContainer? container = null;
        if (this._containers.TryDequeue(out PostgreSqlContainer? container))
        {
            return container;
        }
        
        container = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("refresh")
            .Build();
        
        container.StartAsync().Wait();
        
        this._allContainers.Add(container);
        return container;
    }

    public void Return(PostgreSqlContainer container)
    {
        this._containers.Enqueue(container);
    }

    public void Dispose()
    {
        this._containers.Clear();
        
        foreach (PostgreSqlContainer container in this._allContainers)
        {
            container.DisposeAsync().AsTask().Wait();
        }
        
        this._allContainers.Clear();
        
        GC.SuppressFinalize(this);
    }
}

#endif