using System.Reflection;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.RateLimit;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Middlewares;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer;

public class RefreshGameServer
{
    protected readonly BunkumHttpServer Server = new();

    public RefreshGameServer(
        GameDatabaseProvider? databaseProvider = null,
        IAuthenticationProvider<GameUser, Token>? authProvider = null,
        IDataStore? dataStore = null
    )
    {
        databaseProvider ??= new GameDatabaseProvider();
        authProvider ??= new GameAuthenticationProvider();
        dataStore ??= new FileSystemDataStore();

        this.Server.AddAuthenticationService(authProvider, true);
        this.Server.UseDatabaseProvider(databaseProvider);
        this.Server.AddStorageService(dataStore);
        
        this.Server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void Initialize()
    {
        this.SetupServices();
        this.SetupConfiguration();
        this.SetupMiddlewares();
    }

    protected virtual void SetupMiddlewares()
    {
        this.Server.AddMiddleware<WebsiteMiddleware>();
        this.Server.AddMiddleware<NotFoundLogMiddleware>();
        this.Server.AddMiddleware<DigestMiddleware>();
        this.Server.AddMiddleware<CrossOriginMiddleware>();
    }

    protected virtual void SetupConfiguration()
    {
        this.Server.UseJsonConfig<GameServerConfig>("refreshGameServer.json");
    }

    protected virtual void SetupServices()
    {
        this.Server.AddRateLimitService(new RateLimitSettings(60, 200, 30));
        this.Server.AddService<CategoryService>();
    }

    public Task StartAndBlockAsync()
    {
        return this.Server.StartAndBlockAsync();
    }
}