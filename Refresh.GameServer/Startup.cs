using Bunkum.Core;
using Refresh.Core;
using Refresh.GameServer;

BunkumConsole.AllocateConsole();

#if DEBUG
Console.WriteLine($"Starting Refresh (DEBUG build, version {VersionInformation.Version})");
#else
Console.WriteLine($"Starting Refresh (RELEASE build, version {VersionInformation.Version})");
#endif

using RefreshGameServer server = new();

if (args.Length > 0)
{
    CommandLineManager cli = new(server);
    cli.StartWithArgs(args);
    
    return;
}

server.Start();
await Task.Delay(-1);