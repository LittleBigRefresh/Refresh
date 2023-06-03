using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;
using JetBrains.Annotations;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer;

public class AssetImporter
{
    private readonly GameDatabaseProvider _databaseProvider;
    private readonly IDataStore _dataStore;
    private readonly LoggerContainer<BunkumContext> _logger;
    private readonly Stopwatch _stopwatch;

    public AssetImporter(GameDatabaseProvider databaseProvider, IDataStore dataStore)
    {
        this._databaseProvider = databaseProvider;
        this._dataStore = dataStore;

        this._logger = new LoggerContainer<BunkumContext>();
        this._logger.RegisterLogger(new ConsoleLogger());

        this._stopwatch = new Stopwatch();
    }

    public void ImportFromCli()
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
        
        this.Import();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Info(string message)
    {
        this._logger.LogInfo(BunkumContext.UserContent, $"[{this._stopwatch.ElapsedMilliseconds}ms] {message}");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Warn(string message)
    {
        this._logger.LogWarning(BunkumContext.UserContent, $"[{this._stopwatch.ElapsedMilliseconds}ms] {message}");
    }

    public void Import()
    {
        this._stopwatch.Start();
        
        using GameDatabaseContext context = this._databaseProvider.GetContext();
        context.DeleteAllAssetMetadata();
        this.Info("Deleted all asset metadata");
        
        // TODO: implement getting a list of keys from IDataStore in bunkum!
        // this is a nasty hack! we will have to unit test this eventually, using a file store for tests is a huge no-no
        // eww, manually getting dataStore directory >:(
        List<string> assetHashes = Directory.GetFiles(Path.Join(BunkumFileSystem.DataDirectory, "dataStore"))
            .Select(Path.GetFileName)
            .ToList()!;

        List<GameAsset> assets = new();
        foreach (string hash in assetHashes)
        {
            byte[] data = this._dataStore.GetDataFromStore(hash);
            
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
    private GameAsset? ReadAndVerifyAsset(string hash, byte[] data)
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
            AssetType = GameAssetType.Unknown,
            AssetHash = hash,
        };

        return asset;
    }
}