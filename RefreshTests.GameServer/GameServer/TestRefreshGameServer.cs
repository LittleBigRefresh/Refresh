using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Services;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using JetBrains.Annotations;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using NotEnoughLogs.Sinks;
using Refresh.GameServer;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Levels.Categories;
using RefreshTests.GameServer.Logging;
using RefreshTests.GameServer.Time;

namespace RefreshTests.GameServer.GameServer;

public class TestRefreshGameServer : RefreshGameServer
{
    public TestRefreshGameServer(BunkumHttpListener listener, Func<GameDatabaseProvider> provider, IDataStore? dataStore = null) : base(listener, provider, null, dataStore ?? new InMemoryDataStore())
    {}

    public BunkumHttpServer Server => this._server;

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

    protected override (LoggerConfiguration logConfig, List<ILoggerSink>? sinks) GetLoggerConfiguration()
    {
        LoggerConfiguration logConfig = new()
        {
            Behaviour = new DirectLoggingBehaviour(),
            MaxLevel = LogLevel.Trace,
        };

        List<ILoggerSink> sinks = new(1)
        {
            new NUnitSink(),
        };
        
        return (logConfig, sinks);
    }

    protected override void SetupMiddlewares()
    {
        
    }

    protected override void SetupServices()
    {
        this._server.AddService<TimeProviderService>(this.DateTimeProvider);
        this._server.AddService<CategoryService>();
        this._server.AddService<MatchService>();
        this._server.AddService<ImportService>();
        this._server.AddService<LevelListOverrideService>();
        this._server.AddService<CommandService>();
    }
    
    [Pure]
    public TService GetService<TService>() where TService : Service
    {
        List<Service> services = (List<Service>)typeof(BunkumServer).GetField("_services", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(this._server)!;

        return (TService)services.First(s => typeof(TService) == s.GetType());
    }
}