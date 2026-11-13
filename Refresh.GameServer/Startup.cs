using Bunkum.Core;
using Refresh.GameServer;

BunkumConsole.AllocateConsole();

Console.WriteLine("Starting Refresh with Git-submodule Bunkum");

using RefreshGameServer server = new();

if (args.Length > 0)
{
    CommandLineManager cli = new(server);
    cli.StartWithArgs(args);
    
    return;
}

server.Start();
await Task.Delay(-1);