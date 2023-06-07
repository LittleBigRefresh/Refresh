using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer;
using Refresh.GameServer.Database;
using Refresh.GameServer.Importing;

#if DEBUGLOCALBUNKUM
Console.WriteLine("Starting Refresh with LOCAL Bunkum!");
#elif DEBUG
Console.WriteLine("Starting Refresh with NuGet Bunkum");
#endif

if (args.Length > 0 && args[0] == "--import_assets")
{
    using GameDatabaseProvider provider = new();
    FileSystemDataStore dataStore = new();
    
    provider.Initialize();
    using GameDatabaseContext context = provider.GetContext();
    
    AssetImporter importer = new();
    importer.ImportFromDataStoreCli(context, dataStore);
    return;
}

BunkumConsole.AllocateConsole();

RefreshGameServer server = new();
server.Initialize();

server.Start();
await Task.Delay(-1);