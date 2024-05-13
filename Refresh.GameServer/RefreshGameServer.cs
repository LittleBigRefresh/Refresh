using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Bunkum.AutoDiscover.Extensions;
using Bunkum.Core.Authentication;
using Bunkum.Core.Configuration;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Services;
using Bunkum.Core.Storage;
using Bunkum.HealthChecks;
using Bunkum.HealthChecks.RealmDatabase;
using Bunkum.Protocols.Http;
using Refresh.Common;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation;
using Refresh.GameServer.Endpoints;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Middlewares;
using Refresh.GameServer.Services;
using Refresh.GameServer.Storage;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Workers;

namespace Refresh.GameServer;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RefreshGameServer : RefreshServer
{
    protected WorkerManager? WorkerManager;
    
    protected readonly GameDatabaseProvider _databaseProvider;
    protected readonly IDataStore _dataStore;
    
    protected GameServerConfig? _config;
    protected IntegrationConfig? _integrationConfig;

    public RefreshGameServer(
        BunkumHttpListener? listener = null,
        Func<GameDatabaseProvider>? databaseProvider = null,
        IAuthenticationProvider<Token>? authProvider = null,
        IDataStore? dataStore = null
    ) : base(listener)
    {
        databaseProvider ??= () => new GameDatabaseProvider();
        dataStore ??= new FileSystemDataStore();
        
        this._databaseProvider = databaseProvider.Invoke();
        this._databaseProvider.Initialize();
        this._dataStore = dataStore;
        
        DryArchiveConfig dryConfig = Config.LoadFromJsonFile<DryArchiveConfig>("dry.json", this.Logger);
        if (dryConfig.Enabled)
            this._dataStore = new AggregateDataStore(dataStore, new DryDataStore(dryConfig));
        
        this.SetupInitializer(() =>
        {
            GameDatabaseProvider provider = databaseProvider.Invoke();

            this.WorkerManager?.Stop();
            this.WorkerManager = new WorkerManager(this.Logger, this._dataStore, provider);
            
            authProvider ??= new GameAuthenticationProvider(this._config!);

            this.InjectBaseServices(provider, authProvider, this._dataStore);
        });
    }

    private void InjectBaseServices(GameDatabaseProvider databaseProvider, IAuthenticationProvider<Token> authProvider, IDataStore dataStore)
    {
        this.Server.UseDatabaseProvider(databaseProvider);
        this.Server.AddAuthenticationService(authProvider, true);
        this.Server.AddStorageService(dataStore);
    }

    protected override void Initialize()
    {
        base.Initialize();
        this.SetupWorkers();
    }

    protected override void SetupMiddlewares()
    {
        this.Server.AddMiddleware<LegacyAdapterMiddleware>();
        this.Server.AddMiddleware<WebsiteMiddleware>();
        this.Server.AddMiddleware(new DeflateMiddleware(this._config!));
        this.Server.AddMiddleware<DigestMiddleware>();
        this.Server.AddMiddleware<CrossOriginMiddleware>();
        this.Server.AddMiddleware<PspVersionMiddleware>();
    }

    protected override void SetupConfiguration()
    {
        GameServerConfig config = Config.LoadFromJsonFile<GameServerConfig>("refreshGameServer.json", this.Server.Logger);
        this._config = config;

        IntegrationConfig integrationConfig = Config.LoadFromJsonFile<IntegrationConfig>("integrations.json", this.Server.Logger);
        this._integrationConfig = integrationConfig;
        
        this.Server.AddConfig(config);
        this.Server.AddConfig(integrationConfig);
        this.Server.AddConfigFromJsonFile<RichPresenceConfig>("rpc.json");
        this.Server.AddConfigFromJsonFile<ContactInfoConfig>("contactInfo.json");
    }
    
    protected override void SetupServices()
    {
        this.Server.AddService<TimeProviderService>(this.GetTimeProvider());
        this.Server.AddRateLimitService(new RateLimitSettings(60, 400, 30, "global"));
        this.Server.AddService<CategoryService>();
        this.Server.AddService<MatchService>();
        this.Server.AddService<ImportService>();
        this.Server.AddService<DocumentationService>();
        this.Server.AddService<GuidCheckerService>();
        this.Server.AddAutoDiscover(serverBrand: $"{this._config!.InstanceName} (Refresh)",
            baseEndpoint: GameEndpointAttribute.BaseRoute.Substring(0, GameEndpointAttribute.BaseRoute.Length - 1),
            usesCustomDigestKey: true,
            serverDescription: this._config.InstanceDescription,
            bannerImageUrl: "https://github.com/LittleBigRefresh/Branding/blob/main/logos/refresh_type.png?raw=true");
        
        this.Server.AddHealthCheckService(this._databaseProvider, new []
        {
            typeof(RealmDatabaseHealthCheck),
        });
        
        this.Server.AddService<RoleService>();
        this.Server.AddService<SmtpService>();

        this.Server.AddService<RequestStatisticTrackingService>();
        
        this.Server.AddService<LevelListOverrideService>();
        
        this.Server.AddService<CommandService>();
        
        #if DEBUG
        this.Server.AddService<DebugService>();
        #endif
    }

    protected virtual void SetupWorkers()
    {
        if (this.WorkerManager == null) return;
        
        this.WorkerManager.AddWorker<PunishmentExpiryWorker>();
        this.WorkerManager.AddWorker<ExpiredObjectWorker>();
        this.WorkerManager.AddWorker<CoolLevelsWorker>();
        this.WorkerManager.AddWorker<RequestStatisticSubmitWorker>();
        
        if ((this._integrationConfig?.DiscordWebhookEnabled ?? false) && this._config != null)
        {
            this.WorkerManager.AddWorker(new DiscordIntegrationWorker(this._integrationConfig, this._config));
        }
    }

    /// <inheritdoc/>
    public override void Start()
    {
        this.Server.Start();
        this.WorkerManager?.Start();

        if (this._config!.MaintenanceMode)
        {
            this.Logger.LogWarning(RefreshContext.Startup, "The server is currently in maintenance mode! " +
                                                            "Only administrators will be able to log in and interact with the server.");
        }
    }

    /// <inheritdoc/>
    public override void Stop()
    {
        this.Server.Stop();
        this.WorkerManager?.Stop();
    }

    internal GameDatabaseContext GetContext()
    {
        return this._databaseProvider.GetContext();
    }
    
    protected virtual IDateTimeProvider GetTimeProvider()
    {
        return new SystemDateTimeProvider();
    }

    public void ImportAssets(bool force = false)
    {
        using GameDatabaseContext context = this.GetContext();
        
        AssetImporter importer = new();
        importer.ImportFromDataStore(context, this._dataStore);
    }

    public void ImportImages()
    {
        using GameDatabaseContext context = this.GetContext();
        
        ImageImporter importer = new();
        importer.ImportFromDataStore(context, this._dataStore);
    }

    public void CreateUser(string username, string emailAddress)
    {
        using GameDatabaseContext context = this.GetContext();
        GameUser user = context.CreateUser(username, emailAddress);
        context.VerifyUserEmail(user);
    }
    
    public void SetUserAsRole(GameUser user, GameUserRole role)
    {
        using GameDatabaseContext context = this.GetContext();
        context.SetUserRole(user, role);
    }
    
    public bool DisallowUser(string username)
    {
        using GameDatabaseContext context = this.GetContext();
        
        return context.DisallowUser(username);
    }
    
    public bool ReallowUser(string username)
    {
        using GameDatabaseContext context = this.GetContext();
        
        return context.ReallowUser(username);
    }

    public void RenameUser(GameUser user, string newUsername)
    {
        using GameDatabaseContext context = this.GetContext();
        context.RenameUser(user, newUsername);
    }

    public override void Dispose()
    {
        this._databaseProvider.Dispose();
        base.Dispose();
    }
}