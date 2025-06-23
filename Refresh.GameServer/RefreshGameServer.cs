using System.Diagnostics.CodeAnalysis;
using Bunkum.AutoDiscover.Extensions;
using Bunkum.Core;
using Bunkum.Core.Authentication;
using Bunkum.Core.Configuration;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Services;
using Bunkum.Core.Storage;
using Bunkum.HealthChecks;
using Bunkum.Protocols.Http;
using Refresh.Common;
using Refresh.Common.Time;
using Refresh.Common.Verification;
using Refresh.Core;
using Refresh.Core.Configuration;
using Refresh.Core.Extensions;
using Refresh.Core.Importing;
using Refresh.Core.Services;
using Refresh.Core.Storage;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Data;
using Refresh.GameServer.Authentication;
using Refresh.Database.Models.Authentication;
using Refresh.Database;
using Refresh.GameServer.Middlewares;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.APIv3;
using Refresh.Interfaces.APIv3.Documentation;
using Refresh.Interfaces.Game;
using Refresh.Interfaces.Internal;
using Refresh.Interfaces.Workers;
using Refresh.Interfaces.Workers.RequestTracking;
using Refresh.Interfaces.Workers.Workers;

namespace Refresh.GameServer;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RefreshGameServer : RefreshServer
{
    protected WorkerManager? WorkerManager;
    
    protected readonly GameDatabaseProvider _databaseProvider;
    protected readonly IDataStore _dataStore;
    protected MatchService _matchService = null!;
    protected GuidCheckerService _guidCheckerService = null!;
    
    protected GameServerConfig? _config;
    protected IntegrationConfig? _integrationConfig;

    public RefreshGameServer(
        BunkumHttpListener? listener = null,
        Func<GameDatabaseProvider>? databaseProvider = null,
        IAuthenticationProvider<Token>? authProvider = null,
        IDataStore? dataStore = null
    ) : base(listener)
    {
        dataStore ??= new FileSystemDataStore();

        this._dataStore = dataStore;

        DryArchiveConfig? dryConfig = null;
        try
        {
            dryConfig = Config.LoadFromJsonFile<DryArchiveConfig>("dry.json", this.Logger);
            if (dryConfig.Enabled)
                this._dataStore = new AggregateDataStore(dataStore, new DryDataStore(dryConfig));
        }
        catch (Exception ex)
        {
            this.Logger.LogWarning(BunkumCategory.Configuration, "Failed to read dry.json: " + ex);
        }

        if (databaseProvider == null)
        {
            DatabaseConfig? dbConfig = null;
            try
            {
               dbConfig = Config.LoadFromJsonFile<DatabaseConfig>("db.json", this.Logger);
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical(RefreshContext.Database, "Failed to read the database configuration file: " + ex);
                this.Logger.Dispose();
                Environment.Exit(1);
            }
            
            databaseProvider = () => new GameDatabaseProvider(dbConfig);
        }
        
        this._databaseProvider = databaseProvider.Invoke();
        this._databaseProvider.Initialize();

        // Uncomment if you want to use production refresh as a source for assets
        // TODO: remove config option when test.lbpbonsai.com instance no longer needs prod assets
#if DEBUG
        if (dryConfig?.TemporaryWillBeRemoved_UseProductionRefreshData ?? false)
            this._dataStore = new AggregateDataStore(dataStore, new RemoteRefreshDataStore());
#endif
        
        this.SetupInitializer(() =>
        {
            GameDatabaseProvider provider = databaseProvider.Invoke();

            this.WorkerManager?.Stop();
            
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
        this.Server.DiscoverEndpointsFromAssembly(typeof(ApiV3EndpointAttribute).Assembly);
        this.Server.DiscoverEndpointsFromAssembly(typeof(GameEndpointAttribute).Assembly);
        this.Server.DiscoverEndpointsFromAssembly(typeof(PresenceEndpointAttribute).Assembly);
        this.SetupWorkers();
    }

    protected override void SetupMiddlewares()
    {
        this.Server.AddMiddleware<WebsiteMiddleware>();
        this.Server.AddMiddleware(new DeflateMiddleware(this._config!));
        this.Server.AddMiddleware<LegacyAdapterMiddleware>();
        // Digest middleware must be run before LegacyAdapterMiddleware, because digest is based on the raw route, not the fixed route
        this.Server.AddMiddleware(new DigestMiddleware(this._config!));
        this.Server.AddMiddleware<CrossOriginMiddleware>();
        this.Server.AddMiddleware<PspVersionMiddleware>();
        this.Server.AddMiddleware(new PresenceAuthenticationMiddleware(this._integrationConfig!));
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
        this.Server.AddService(this._matchService = new MatchService(this.Server.Logger, this._config!));
        this.Server.AddService<ImportService>();
        this.Server.AddService<DocumentationService>();
        this.Server.AddService(this._guidCheckerService = new GuidCheckerService(this._config!, this.Server.Logger));
        this.Server.AddAutoDiscover(serverBrand: $"{this._config!.InstanceName} (Refresh)",
            baseEndpoint: GameEndpointAttribute.BaseRoute.Substring(0, GameEndpointAttribute.BaseRoute.Length - 1),
            usesCustomDigestKey: true,
            serverDescription: this._config.InstanceDescription,
            bannerImageUrl: "https://github.com/LittleBigRefresh/Branding/blob/main/logos/refresh_type.png?raw=true");
        
#pragma warning disable CA1825
#pragma warning disable CA1861
        this.Server.AddHealthCheckService(this._databaseProvider, new Type[]
        {
            // TODO: add postgres health check
        });
#pragma warning restore CA1861
#pragma warning restore CA1825
        
        this.Server.AddService<RoleService>();
        this.Server.AddService<SmtpService>();
        this.Server.AddService<RequestStatisticTrackingService>();
        this.Server.AddService<PresenceService>();
        this.Server.AddService<PlayNowService>();
        this.Server.AddService<CommandService>();
        this.Server.AddService<ChallengeGhostRateLimitService>();
        this.Server.AddService<DiscordStaffService>();

        if(this._integrationConfig!.AipiEnabled)
            this.Server.AddService<AipiService>();
        
        #if DEBUG
        this.Server.AddService<DebugService>();
        #endif
        
        // !!! HEY! !!!
        // This service depends on most services that come before it.
        // This should always be added last.
        this.Server.AddService<DataContextService>();
    }

    protected virtual void SetupWorkers()
    {
        this.WorkerManager = new WorkerManager(this.Logger, this._dataStore, this._databaseProvider, this._matchService, this._guidCheckerService);
        
        this.WorkerManager.AddWorker<PunishmentExpiryWorker>();
        this.WorkerManager.AddWorker<ExpiredObjectWorker>();
        this.WorkerManager.AddWorker<CoolLevelsWorker>();
        this.WorkerManager.AddWorker<RequestStatisticSubmitWorker>();
        
        if ((this._integrationConfig?.DiscordWebhookEnabled ?? false) && this._config != null && this._config.PermitShowingOnlineUsers)
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
        AssetImporter importer = new();
        importer.ImportFromDataStore(this._databaseProvider, this._dataStore);
    }

    public void ImportImages()
    {
        using GameDatabaseContext context = this.GetContext();
        
        ImageImporter importer = new();
        importer.ImportFromDataStore(context, this._dataStore);
    }

    public void FlagModdedLevels()
    {
        using GameDatabaseContext context = this.GetContext();

        // Iterate over all levels
        GameLevel[] allLevels = context.GetAllUserLevels().ToArray();
        Dictionary<GameLevel, bool> statuses = new(allLevels.Length);
        int i = 0;
        foreach (GameLevel level in allLevels)
        {
            bool modded = context.GetLevelModdedStatus(level);
            
            statuses[level] = modded;

            i++;
            this.Logger.LogInfo(RefreshContext.Worker, "[{3}/{4}] Marked level {0} ({1}) as {2}", level.Title, level.LevelId, modded ? "modded" : "vanilla", i, allLevels.Length);
        }
        
        context.SetLevelModdedStatuses(statuses);
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
    
    public void DeleteUser(GameUser user)
    {
        using GameDatabaseContext context = this.GetContext();
        context.DeleteUser(user);
    }
    
    public void FullyDeleteUser(GameUser user)
    {
        using GameDatabaseContext context = this.GetContext();
        context.FullyDeleteUser(user);
    }
    
    public void MarkAllReuploads(GameUser user)
    {
        using GameDatabaseContext context = this.GetContext();
        context.MarkAllReuploads(user);
    }

    public string AskUserForVerification(GameUser user)
    {
        using GameDatabaseContext context = this.GetContext();
        string code = CodeHelper.GenerateDigitCode();

        string text = $"An admin is requesting a code to verify that you currently have access to your account. " +
                      $"Please share the code '{code}' with the admin you're currently speaking with. " +
                      "If you're not in contact with any such staff member, please report this immediately.";
        
        context.AddNotification("Admin Verification Request (Action Required)", text, user, "shield");

        return code;
    }

    public override void Dispose()
    {
        this._databaseProvider.Dispose();
        base.Dispose();
    }
}