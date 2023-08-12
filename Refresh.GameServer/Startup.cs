using Bunkum.HttpServer;
using Refresh.GameServer;

BunkumConsole.AllocateConsole();

#if DEBUGLOCALBUNKUM
Console.WriteLine("Starting Refresh with LOCAL Bunkum!");
#elif DEBUG
Console.WriteLine("Starting Refresh with NuGet Bunkum");
#endif

RefreshGameServer server = new();

if (args.Length > 0)
{
    CommandLineManager cli = new(server);
    cli.StartWithArgs(args);
    return;
}

server.Start();
await Task.Delay(-1);