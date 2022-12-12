using System.Reflection;
using Refresh.HttpServer;
using RefreshTests.HttpServer;

RefreshHttpServer server = new("http://+:10060/");
server.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);

await server.StartAndBlockAsync();