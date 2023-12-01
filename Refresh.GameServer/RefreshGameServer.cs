using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Bunkum.AutoDiscover.Extensions;
using Bunkum.Core.Authentication;
using Bunkum.Core.Configuration;
using Bunkum.Core.Database;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Services;
using Bunkum.Core.Storage;
using Bunkum.HealthChecks;
using Bunkum.HealthChecks.RealmDatabase;
using Bunkum.Protocols.Http;
using Microsoft.EntityFrameworkCore;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using NotEnoughLogs.Sinks;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Database.Postgres;
using Refresh.GameServer.Database.Realm;
using Refresh.GameServer.Documentation;
using Refresh.GameServer.Endpoints;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Middlewares;
using Refresh.GameServer.Services;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Workers;

namespace Refresh.GameServer;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RefreshGameServer : IDisposable
{
    public Logger Logger => this._server.Logger;
    
    protected readonly BunkumHttpServer _server;
    protected WorkerManager? _workerManager;
    
    protected readonly IDatabaseProvider<IGameDatabaseContext> _databaseProvider;
    protected readonly IDataStore _dataStore;
    
    protected GameServerConfig? _config;
    protected IntegrationConfig? _integrationConfig;

    public RefreshGameServer(
        BunkumHttpListener? listener = null,
        Func<IDatabaseProvider<IGameDatabaseContext>>? databaseProvider = null,
        IAuthenticationProvider<Token>? authProvider = null,
        IDataStore? dataStore = null
    )
    {
        // databaseProvider ??= () => new RealmGameDatabaseProvider();
        databaseProvider ??= () => new PostgresGameDatabaseProvider((options) =>
        {
            options.UseNpgsql();
        });
        dataStore ??= new FileSystemDataStore();

        // ReSharper disable once VirtualMemberCallInConstructor
        (LoggerConfiguration logConfig, List<ILoggerSink>? sinks) = this.GetLoggerConfiguration();
        
        this._databaseProvider = databaseProvider.Invoke();
        this._dataStore = dataStore;
        
        this._server = listener == null ? new BunkumHttpServer(logConfig, sinks) : new BunkumHttpServer(listener, configuration: logConfig, sinks);
        this.Logger.LogDebug(RefreshContext.Startup, "Successfully initialized " + this.GetType().Name);
        
        this._server.Initialize = _ =>
        {
            IDatabaseProvider<IGameDatabaseContext> provider = databaseProvider.Invoke();
            
            this._workerManager?.Stop();
            this._workerManager = new WorkerManager(this.Logger, this._dataStore, provider);
            
            this.SetupConfiguration();
            authProvider ??= new GameAuthenticationProvider(this._config!);
            
            this.InjectBaseServices(provider, authProvider, dataStore);
            this.Initialize();
        };
        
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private void InjectBaseServices(IDatabaseProvider<IGameDatabaseContext> databaseProvider, IAuthenticationProvider<Token> authProvider, IDataStore dataStore)
    {
        this._server.UseDatabaseProvider(databaseProvider);
        this._server.AddAuthenticationService(authProvider, true);
        this._server.AddStorageService(dataStore);
    }

    private void Initialize()
    {
        this.SetupServices();
        this.SetupMiddlewares();
        this.SetupWorkers();
        
        this._server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected virtual void SetupMiddlewares()
    {
        this._server.AddMiddleware<ApiV2GoneMiddleware>();
        this._server.AddMiddleware<LegacyAdapterMiddleware>();
        this._server.AddMiddleware<WebsiteMiddleware>();
        this._server.AddMiddleware<DigestMiddleware>();
        this._server.AddMiddleware<CrossOriginMiddleware>();
        this._server.AddMiddleware<PspVersionMiddleware>();
    }

    protected virtual void SetupConfiguration()
    {
        GameServerConfig config = Config.LoadFromJsonFile<GameServerConfig>("refreshGameServer.json", this._server.Logger);
        this._config = config;

        IntegrationConfig integrationConfig = Config.LoadFromJsonFile<IntegrationConfig>("integrations.json", this._server.Logger);
        this._integrationConfig = integrationConfig;
        
        this._server.AddConfig(config);
        this._server.AddConfig(integrationConfig);
        this._server.AddConfigFromJsonFile<RichPresenceConfig>("rpc.json");
    }
    
    protected virtual void SetupServices()
    {
        this._server.AddService<TimeProviderService>(this.GetTimeProvider());
        this._server.AddRateLimitService(new RateLimitSettings(60, 400, 30, "global"));
        this._server.AddService<CategoryService>();
        this._server.AddService<FriendStorageService>();
        this._server.AddService<MatchService>();
        this._server.AddService<ImportService>();
        this._server.AddService<DocumentationService>();
        this._server.AddAutoDiscover(serverBrand: $"{this._config!.InstanceName} (Refresh)",
            baseEndpoint: GameEndpointAttribute.BaseRoute.Substring(0, GameEndpointAttribute.BaseRoute.Length - 1),
            usesCustomDigestKey: true,
            serverDescription: this._config.InstanceDescription,
            bannerImageUrl: "https://github.com/LittleBigRefresh/Branding/blob/main/logos/refresh_type.png?raw=true");
        
        this._server.AddHealthCheckService(this._databaseProvider, new []
        {
            typeof(RealmDatabaseHealthCheck),
        });
        
        this._server.AddService<RoleService>();
        this._server.AddService<SmtpService>();

        if (this._config!.TrackRequestStatistics)
            this._server.AddService<RequestStatisticTrackingService>();
        
        this._server.AddService<LevelListOverrideService>();
        
        this._server.AddService<CommandService>();
        
        #if DEBUG
        this._server.AddService<DebugService>();
        #endif
    }

    protected virtual void SetupWorkers()
    {
        if (this._workerManager == null) return;
        
        this._workerManager.AddWorker<PunishmentExpiryWorker>();
        this._workerManager.AddWorker<ExpiredObjectWorker>();
        this._workerManager.AddWorker<CoolLevelsWorker>();
        
        if ((this._integrationConfig?.DiscordWebhookEnabled ?? false) && this._config != null)
        {
            this._workerManager.AddWorker(new DiscordIntegrationWorker(this._integrationConfig, this._config));
        }
    }

    public virtual void Start()
    {
        this._server.Start();
        this._workerManager?.Start();

        if (this._config!.MaintenanceMode)
        {
            this.Logger.LogWarning(RefreshContext.Startup, "The server is currently in maintenance mode! " +
                                                            "Only administrators will be able to log in and interact with the server.");
        }
    }

    public void Stop()
    {
        this._server.Stop();
        this._workerManager?.Stop();
    }

    private IGameDatabaseContext InitializeDatabase()
    {
        this._databaseProvider.Initialize();
        return this._databaseProvider.GetContext();
    }
    
    protected virtual IDateTimeProvider GetTimeProvider()
    {
        return new SystemDateTimeProvider();
    }
    
    protected virtual (LoggerConfiguration logConfig, List<ILoggerSink>? sinks) GetLoggerConfiguration()
    {
        LoggerConfiguration logConfig = new()
        {
            Behaviour = new QueueLoggingBehaviour(),
            #if DEBUG
            MaxLevel = LogLevel.Debug,
            #else
            MaxLevel = LogLevel.Info,
            #endif
        };
        
        return (logConfig, null);
    }

    public void ImportAssets(bool force = false)
    {
        using IGameDatabaseContext context = this.InitializeDatabase();
        
        AssetImporter importer = new();
        importer.ImportFromDataStore(context, this._dataStore);
    }

    public void ImportImages()
    {
        using IGameDatabaseContext context = this.InitializeDatabase();
        
        ImageImporter importer = new();
        importer.ImportFromDataStore(context, this._dataStore);
    }

    public void CreateUser(string username, string emailAddress)
    {
        using IGameDatabaseContext context = this.InitializeDatabase();
        GameUser user = context.CreateUser(username, emailAddress);
        context.VerifyUserEmail(user);
    }
    
    public void SetAdminFromUsername(string username)
    {
        using IGameDatabaseContext context = this.InitializeDatabase();

        GameUser? user = context.GetUserByUsername(username);
        if (user == null) throw new InvalidOperationException("Cannot find the user " + username);

        context.SetUserRole(user, GameUserRole.Admin);
    }
    
    public void SetAdminFromEmailAddress(string emailAddress)
    {
        using IGameDatabaseContext context = this.InitializeDatabase();

        GameUser? user = context.GetUserByEmailAddress(emailAddress);
        if (user == null) throw new InvalidOperationException("Cannot find a user by emailAddress " + emailAddress);

        context.SetUserRole(user, GameUserRole.Admin);
    }

    public void Dispose()
    {
        this.Logger.Dispose();
        this._databaseProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}