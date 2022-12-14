using System.Reflection;
using Refresh.GameServer;
using Refresh.GameServer.Database;
using Refresh.HttpServer;

RefreshHttpServer server = new("http://+:10061/");

server.UseDatabaseProvider(new RealmDatabaseProvider());
server.UseAuthenticationProvider(new GameAuthenticationProvider());

server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());

await server.StartAndBlockAsync();