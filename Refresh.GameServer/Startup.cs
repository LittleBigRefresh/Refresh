using Bunkum.Core;
using Refresh.GameServer;

BunkumConsole.AllocateConsole();

#if DEBUGLOCALBUNKUM
Console.WriteLine("Starting Refresh with LOCAL Bunkum!");
#elif DEBUG
Console.WriteLine("Starting Refresh with NuGet Bunkum");
#endif

using RefreshGameServer server = new();

if (args.Length > 0)
{
    CommandLineManager cli = new(server);
    cli.StartWithArgs(args);

    server.Dispose();
    return;
}

server.Start();
await Task.Delay(-1);