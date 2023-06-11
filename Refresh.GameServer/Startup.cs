using System.Net.Mime;
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

Console.ReadKey(true);

if (args.Length > 0)
{
    using GameDatabaseProvider provider = new();
    FileSystemDataStore dataStore = new();
    
    provider.Initialize();
    using GameDatabaseContext context = provider.GetContext();

    if (args[0] == "--import_assets")
    {
        AssetImporter importer = new();
        importer.ImportFromDataStoreCli(context, dataStore);
    }
    else if (args[0] == "--import_images")
    {
        ImageImporter importer = new();
        importer.ImportFromDataStore(context, dataStore);
    }
    else
    {
        Console.WriteLine("Bad argument");
        Environment.Exit(1);
    }
    return;
}

BunkumConsole.AllocateConsole();

RefreshGameServer server = new();
server.Initialize();

server.Start();
await Task.Delay(-1);