using System.Reflection;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.HttpServer;
using Refresh.HttpServer.Storage;

RefreshHttpServer server = new("http://+:10061/")
{
    AssumeAuthenticationRequired = true,
};

server.UseDatabaseProvider(new RealmDatabaseProvider());
server.UseAuthenticationProvider(new GameAuthenticationProvider());
server.UseDataStore(new FileSystemDataStore());

server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());

#region Log unimplemented endpoints
#if DEBUG

const string endpointFile = "unimplementedEndpoints.txt";
if(!File.Exists(endpointFile)) File.WriteAllText(endpointFile, string.Empty);
List<string> unimplementedEndpoints = File.ReadAllLines(endpointFile).ToList();

server.NotFound += (sender, context) =>
{
    if (context.Request.Url == null) return;
    Uri url = context.Request.Url;

    if (unimplementedEndpoints.Any(e => e.Split('?')[0] == url.AbsolutePath)) return;

    unimplementedEndpoints.Add(url.PathAndQuery);
    File.WriteAllLines(endpointFile, unimplementedEndpoints);
};

#endif
#endregion

await server.StartAndBlockAsync();