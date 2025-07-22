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
using Refresh.Core.Metrics;
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
using Refresh.Interfaces.Workers.Repeating;
using Refresh.Workers;

namespace Refresh.GameServer;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RefreshGameServer : RefreshServer
{
    protected WorkerManager? WorkerManager;
    
    protected readonly GameDatabaseProvider _databaseProvider;
    protected readonly IDataStore _dataStore;
    protected readonly ConfigStore _configStore;

    public RefreshGameServer(
        BunkumHttpListener? listener = null,
        Func<GameDatabaseProvider>? databaseProvider = null,
        IAuthenticationProvider<Token>? authProvider = null,
        IDataStore? dataStore = null
    ) : base(listener)
    {
        dataStore ??= new FileSystemDataStore();
        List<IDataStore> dataStores = [];

        try
        {
            this._configStore = new ConfigStore(this.Logger);
        }
        catch (Exception ex)
        {
            this.Logger.LogCritical(RefreshContext.Database, "Failed to read the configuration files: " + ex);
            this.Logger.Dispose();
            Environment.Exit(1);
        }
        
        if (this._configStore.DryArchive.Enabled)
            dataStores.Add(new DownloadingDataStore(dataStore, new DryDataStore(this._configStore.DryArchive)));

        databaseProvider ??= () => new GameDatabaseProvider(this.Logger, this._configStore.Database);
        
        this._databaseProvider = databaseProvider.Invoke();
        this._databaseProvider.Initialize();

        // Uncomment if you want to use production refresh as a source for assets
        // TODO: remove config option when test.lbpbonsai.com instance no longer needs prod assets
#if DEBUG
        if (this._configStore.DryArchive?.TemporaryWillBeRemoved_UseProductionRefreshData ?? false)
            dataStores.Add(new DownloadingDataStore(dataStore, new RemoteRefreshDataStore()));
#endif

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (dataStores.Count > 0)
            this._dataStore = new AggregateDataStore(dataStore, dataStores.ToArray());
        else
            this._dataStore = dataStore;

        this.SetupInitializer(() =>
        {
            GameDatabaseProvider provider = databaseProvider.Invoke();

            this.WorkerManager?.Stop();
            
            authProvider ??= new GameAuthenticationProvider(this._configStore.GameServer);

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
        this.Server.AddMiddleware(new DeflateMiddleware(this._configStore.GameServer));
        this.Server.AddMiddleware<LegacyAdapterMiddleware>();
        // Digest middleware must be run before LegacyAdapterMiddleware, because digest is based on the raw route, not the fixed route
        this.Server.AddMiddleware(new DigestMiddleware(this._configStore.GameServer));
        this.Server.AddMiddleware<CrossOriginMiddleware>();
        this.Server.AddMiddleware<PspVersionMiddleware>();
        this.Server.AddMiddleware(new PresenceAuthenticationMiddleware(this._configStore.Integration!));
        this.Server.AddMiddleware<RequestStatisticTrackingMiddleware>();
    }

    protected override void SetupConfiguration()
    {
        this._configStore.AddToBunkum(this.Server);
    }
    
    protected override void SetupServices()
    {
        this.Server.AddService<TimeProviderService>(this.GetTimeProvider());
        this.Server.AddRateLimitService(new RateLimitSettings(60, 400, 30, "global"));
        this.Server.AddService<CategoryService>();
        this.Server.AddService(new MatchService(this.Server.Logger, this._configStore.GameServer));
        this.Server.AddService<ImportService>();
        this.Server.AddService<DocumentationService>();
        this.Server.AddService(new GuidCheckerService(this._configStore.GameServer, this.Server.Logger));
        this.Server.AddAutoDiscover(serverBrand: $"{this._configStore.GameServer.InstanceName} (Refresh)",
            baseEndpoint: GameEndpointAttribute.BaseRoute[..^1],
            usesCustomDigestKey: true,
            serverDescription: this._configStore.GameServer.InstanceDescription,
            bannerImageUrl: "https://github.com/LittleBigRefresh/Branding/blob/main/logos/refresh_type.png?raw=true");
        
#pragma warning disable CA1825
#pragma warning disable CA1861
        this.Server.AddHealthCheckService(this._databaseProvider, [
            // TODO: add postgres health check
        ]);
#pragma warning restore CA1861
#pragma warning restore CA1825
        
        this.Server.AddService<RoleService>();
        this.Server.AddService<SmtpService>();
        this.Server.AddService<PresenceService>();
        this.Server.AddService<PlayNowService>();
        this.Server.AddService<CommandService>();
        this.Server.AddService<ChallengeGhostRateLimitService>();
        this.Server.AddService<DiscordStaffService>();

        if(this._configStore.Integration!.AipiEnabled)
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
        this.WorkerManager = RefreshWorkerManager.Create(this.Logger, this._dataStore, this._databaseProvider);
        
        if (this._configStore.Integration.DiscordWebhookEnabled && this._configStore.GameServer.PermitShowingOnlineUsers)
        {
            this.WorkerManager.AddJob(new DiscordIntegrationJob(this._configStore.Integration, this._configStore.GameServer));
        }
    }

    /// <inheritdoc/>
    public override void Start()
    {
        this.Server.Start();
        this.WorkerManager?.Start();

        if (this._configStore.GameServer.MaintenanceMode)
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