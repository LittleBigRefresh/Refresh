using System.Reflection;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;

#if DEBUGLOCALBUNKUM
Console.WriteLine("Starting Refresh with LOCAL Bunkum!");
#elif DEBUG
Console.WriteLine("Starting Refresh with NuGet Bunkum");
#endif

BunkumConsole.AllocateConsole();

BunkumHttpServer server = new(new Uri("http://0.0.0.0:10061/"))
{
    AssumeAuthenticationRequired = true,
    UseDigestSystem = true,
};

using RealmDatabaseProvider databaseProvider = new();

server.UseDatabaseProvider(databaseProvider);
server.UseAuthenticationProvider(new GameAuthenticationProvider());
server.UseDataStore(new FileSystemDataStore());
server.UseJsonConfig<GameServerConfig>("refreshGameServer.json");

server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());

#region Log unimplemented endpoints
#if DEBUG

const string endpointFile = "unimplementedEndpoints.txt";
if(!File.Exists(endpointFile)) File.WriteAllText(endpointFile, string.Empty);
List<string> unimplementedEndpoints = File.ReadAllLines(endpointFile).ToList();

server.NotFound += (_, context) =>
{
    if (unimplementedEndpoints.Any(e => e.Split('?')[0] == context.Uri.AbsolutePath)) return;

    unimplementedEndpoints.Add(context.Uri.PathAndQuery);
    File.WriteAllLines(endpointFile, unimplementedEndpoints);
};

#endif
#endregion

await server.StartAndBlockAsync();