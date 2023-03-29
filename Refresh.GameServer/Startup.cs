using System.Diagnostics;
using System.Reflection;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Middlewares;

#if DEBUGLOCALBUNKUM
Console.WriteLine("Starting Refresh with LOCAL Bunkum!");
#elif DEBUG
Console.WriteLine("Starting Refresh with NuGet Bunkum");
#endif

BunkumConsole.AllocateConsole();

BunkumHttpServer server = new()
{
    AssumeAuthenticationRequired = true,
};

using RealmDatabaseProvider databaseProvider = new();

server.UseDatabaseProvider(databaseProvider);
server.UseAuthenticationProvider(new GameAuthenticationProvider());
server.UseDataStore(new FileSystemDataStore());
server.UseJsonConfig<GameServerConfig>("refreshGameServer.json");

server.AddMiddleware<WebsiteMiddleware>();
server.AddMiddleware<NotFoundLogMiddleware>();
server.AddMiddleware<DigestMiddleware>();
server.AddMiddleware<CrossOriginMiddleware>();

server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());

await server.StartAndBlockAsync();