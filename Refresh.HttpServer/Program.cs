namespace Refresh.HttpServer;

public static class Program
{
    public static async Task Main()
    {
        RefreshHttpServer server = new(new Uri("http://localhost:10060"));
        await server.StartAsync();
    }
}