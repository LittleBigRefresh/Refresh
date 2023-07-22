using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Bunkum.CustomHttpListener;
using Bunkum.AutoDiscover.Extensions;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.RateLimit;
using Bunkum.HttpServer.Storage;
using Bunkum.RealmDatabase;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation;
using Refresh.GameServer.Endpoints;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Middlewares;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class RefreshGameServer
{
    protected readonly BunkumHttpServer _server;
    protected GameDatabaseProvider _databaseProvider;
    protected readonly IDataStore _dataStore;

    public RefreshGameServer(
        BunkumHttpListener? listener = null,
        Func<GameDatabaseProvider>? databaseProvider = null,
        IAuthenticationProvider<GameUser, Token>? authProvider = null,
        IDataStore? dataStore = null
    )
    {
        databaseProvider ??= () => new GameDatabaseProvider();
        authProvider ??= new GameAuthenticationProvider();
        dataStore ??= new FileSystemDataStore();

        this._databaseProvider = databaseProvider.Invoke();
        this._dataStore = dataStore;

        this._server = listener == null ? new BunkumHttpServer() : new BunkumHttpServer(listener);
        
        this._server.Initialize = () =>
        {
            this.InjectBaseServices(databaseProvider.Invoke(), authProvider, dataStore);
            this.Initialize();
        };
    }

    private void InjectBaseServices(GameDatabaseProvider databaseProvider, IAuthenticationProvider<GameUser, Token> authProvider, IDataStore dataStore)
    {
        this._server.AddAuthenticationService(authProvider, true);
        this._server.UseDatabaseProvider(databaseProvider);
        this._server.AddStorageService(dataStore);
    }

    private void Initialize()
    {
        this.SetupServices();
        this.SetupConfiguration();
        this.SetupMiddlewares();
        
        this._server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected virtual void SetupMiddlewares()
    {
        this._server.AddMiddleware<ApiV2GoneMiddleware>();
        this._server.AddMiddleware<LegacyAdapterMiddleware>();
        this._server.AddMiddleware<WebsiteMiddleware>();
        this._server.AddMiddleware<NotFoundLogMiddleware>();
        this._server.AddMiddleware<DigestMiddleware>();
        this._server.AddMiddleware<CrossOriginMiddleware>();
    }

    protected virtual void SetupConfiguration()
    {
        this._server.UseJsonConfig<GameServerConfig>("refreshGameServer.json");
    }

    protected virtual void SetupServices()
    {
        this._server.AddRateLimitService(new RateLimitSettings(60, 200, 30));
        this._server.AddService<CategoryService>();
        this._server.AddService<FriendStorageService>();
        this._server.AddService<MatchService>();
        this._server.AddService<ImportService>();
        this._server.AddService<DocumentationService>();
        this._server.AddAutoDiscover(serverBrand: "Refresh",
            baseEndpoint: GameEndpointAttribute.BaseRoute.Substring(0, GameEndpointAttribute.BaseRoute.Length - 1),
            usesCustomDigestKey: true);
        
        this._server.AddHealthCheckService(new []
        {
            typeof(RealmDatabaseHealthCheck),
        });
        
    }

    public Task StartAndBlockAsync()
    {
        return this._server.StartAndBlockAsync();
    }

    public virtual void Start()
    {
        this._server.Start();
    }

    public void Stop()
    {
        this._server.Stop();
    }

    private GameDatabaseContext InitializeDatabase()
    {
        this._databaseProvider.Initialize();
        return this._databaseProvider.GetContext();
    }

    public void ImportAssets(bool force = false)
    {
        using GameDatabaseContext context = this.InitializeDatabase();
        
        AssetImporter importer = new();
        if (!force)
        {
            importer.ImportFromDataStoreCli(context, this._dataStore);
        }
        else
        {
            importer.ImportFromDataStore(context, this._dataStore);
        }
    }

    public void ImportImages()
    {
        using GameDatabaseContext context = this.InitializeDatabase();
        
        ImageImporter importer = new();
        importer.ImportFromDataStore(context, this._dataStore);
    }
}