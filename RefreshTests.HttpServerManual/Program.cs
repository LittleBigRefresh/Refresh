using Refresh.HttpServer;
using RefreshTests.HttpServer;

RefreshConsole.AllocateConsole();

RefreshHttpServer server = new("http://+:10060/");
server.DiscoverEndpointsFromAssembly(typeof(ServerDependentTest).Assembly);

await server.StartAndBlockAsync();