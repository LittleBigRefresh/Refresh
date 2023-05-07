using System.Reflection;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Bunkum.HttpServer;
using Bunkum.HttpServer.RateLimit;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Middlewares;
using Refresh.GameServer.Types.Levels.Categories;

#if DEBUGLOCALBUNKUM
Console.WriteLine("Starting Refresh with LOCAL Bunkum!");
#elif DEBUG
Console.WriteLine("Starting Refresh with NuGet Bunkum");
#endif

BunkumConsole.AllocateConsole();

BunkumHttpServer server = new();

using GameDatabaseProvider databaseProvider = new();

server.UseDatabaseProvider(databaseProvider);

server.AddAuthenticationService(new GameAuthenticationProvider(), true);
server.AddStorageService<FileSystemDataStore>();
server.AddRateLimitService(new RateLimitSettings(60, 200, 30));
server.AddService<CategoryService>();

server.UseJsonConfig<GameServerConfig>("refreshGameServer.json");

server.AddMiddleware<WebsiteMiddleware>();
server.AddMiddleware<NotFoundLogMiddleware>();
server.AddMiddleware<DigestMiddleware>();
server.AddMiddleware<CrossOriginMiddleware>();

server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());

await server.StartAndBlockAsync();