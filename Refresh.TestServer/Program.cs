using Refresh.HttpServer;
using Refresh.TestServer.Endpoints;

RefreshHttpServer server = new(new Uri("http://localhost:10060"));
server.AddEndpoint<TestEndpoint>();
await server.StartAndBlockAsync();