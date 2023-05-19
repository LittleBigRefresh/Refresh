using Bunkum.HttpServer;
using Refresh.GameServer;

#if DEBUGLOCALBUNKUM
Console.WriteLine("Starting Refresh with LOCAL Bunkum!");
#elif DEBUG
Console.WriteLine("Starting Refresh with NuGet Bunkum");
#endif

BunkumConsole.AllocateConsole();

RefreshGameServer server = new();
server.Initialize();

server.Start();
await Task.Delay(-1);