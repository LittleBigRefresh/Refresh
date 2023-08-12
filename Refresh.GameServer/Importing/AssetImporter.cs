using System.Security.Cryptography;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;
using JetBrains.Annotations;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Importing;

public class AssetImporter : Importer
{
    public AssetImporter(LoggerContainer<BunkumContext>? logger = null) : base(logger)
    {}

    public void ImportFromDataStoreCli(GameDatabaseContext context, IDataStore dataStore)
    {
        Console.WriteLine("This tool will scan and manually import existing assets into Refresh's database.");
        Console.WriteLine("This will wipe all existing asset metadata in the database. Are you sure you want to follow through with this operation?");
        Console.WriteLine();
        Console.Write("Are you sure? [y/N] ");
        
        char key = char.ToLower(Console.ReadKey().KeyChar);
        Console.WriteLine();
        if(key != 'y')
        {
            if(key != 'n') Console.WriteLine("Unsure what you mean, assuming no.");
            Environment.Exit(0);
            return;
        }
        
        this.ImportFromDataStore(context, dataStore);
    }

    public void ImportFromDataStore(GameDatabaseContext context, IDataStore dataStore)
    {
        this.Stopwatch.Start();
        
        context.DeleteAllAssetMetadata();
        this.Info("Deleted all asset metadata");
        
        List<string> assetHashes = dataStore.GetKeysFromStore()
            .Where(key => !key.Contains('/'))
            .ToList();

        List<GameAsset> assets = new();
        foreach (string hash in assetHashes)
        {
            byte[] data = dataStore.GetDataFromStore(hash);
            
            GameAsset? asset = this.ReadAndVerifyAsset(hash, data);
            if (asset == null) continue;

            assets.Add(asset);
            this.Info($"Processed {asset.AssetType} asset {hash} ({AssetSafetyLevelExtensions.FromAssetType(asset.AssetType)})");
        }
        
        context.AddAssetsToDatabase(assets);
        
        this.Info($"Successfully imported {assets.Count}/{assetHashes.Count} assets into database");
        if (assets.Count < assetHashes.Count)
        {
            this.Warn($"{assetHashes.Count - assets.Count} assets were not imported");
        }
    }

    [Pure]
    public GameAsset? ReadAndVerifyAsset(string hash, byte[] data)
    {
        string checkedHash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        if (checkedHash != hash)
        {
            this.Warn($"{hash} is actually hashed as {checkedHash} - this asset is likely corrupt.");
            return null;
        }

        GameAsset asset = new()
        {
            UploadDate = DateTimeOffset.Now,
            OriginalUploader = null,
            AssetHash = hash,
            AssetType = DetermineAssetType(data),
        };

        return asset;
    }
}