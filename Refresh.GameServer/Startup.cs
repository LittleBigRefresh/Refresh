using Bunkum.Core;
using Refresh.Common;
using Refresh.GameServer;

BunkumConsole.AllocateConsole();

string buildType
#if DEBUG
    = "DEBUG";
#else
    = "RELEASE";
#endif
Console.WriteLine($"Starting the Refresh Game server ({buildType} build, version {VersionInformation.Version})...");

using RefreshGameServer server = new();

if (args.Length > 0)
{
    CommandLineManager cli = new(server);
    cli.StartWithArgs(args);
    
    return;
}

server.Start();
await Task.Delay(-1);