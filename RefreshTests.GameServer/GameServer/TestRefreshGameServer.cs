using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using NotEnoughLogs;
using Refresh.GameServer;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Levels.Categories;
using RefreshTests.GameServer.Time;

namespace RefreshTests.GameServer.GameServer;

public class TestRefreshGameServer : RefreshGameServer
{
    public TestRefreshGameServer(BunkumHttpListener listener, Func<GameDatabaseProvider> provider) : base(listener, provider, null, new InMemoryDataStore())
    {}

    public BunkumHttpServer Server => this._server;
    public Logger Logger => base.Logger;

    protected override void SetupConfiguration()
    {
        this._server.AddConfig(new GameServerConfig());
        this._server.AddConfig(new RichPresenceConfig());
        this._server.AddConfig(new IntegrationConfig());
    }

    public override void Start()
    {
        this._server.Start(0);
        // this._workerManager.Start();
    }

    public IDateTimeProvider DateTimeProvider { get; set; } = new MockDateTimeProvider();
    
    protected override IDateTimeProvider GetTimeProvider() => this.DateTimeProvider;

    protected override void SetupMiddlewares()
    {
        
    }

    protected override void SetupServices()
    {
        this._server.AddService<TimeProviderService>(this.DateTimeProvider);
        this._server.AddService<CategoryService>();
        this._server.AddService<MatchService>();
    }
}