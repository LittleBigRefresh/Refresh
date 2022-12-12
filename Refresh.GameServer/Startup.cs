using System.Reflection;
using Refresh.HttpServer;

RefreshHttpServer server = new("http://+:10061/");
server.DiscoverEndpointsFromAssembly(Assembly.GetExecutingAssembly());

await server.StartAndBlockAsync();