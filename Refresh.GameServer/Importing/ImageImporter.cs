using System.Collections.Concurrent;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Importing;

public partial class ImageImporter : Importer
{
    public ImageImporter(Logger? logger = null) : base(logger)
    {}

    public void ImportFromDataStore(GameDatabaseContext context, IDataStore dataStore)
    {
        this.Stopwatch.Start();

        List<GameAsset> assets = new();

        assets.AddRange(context.GetAssetsByType(GameAssetType.Texture));
        // TODO: assets.AddRange(context.GetAssetsByType(GameAssetType.GameDataTexture));
        assets.AddRange(context.GetAssetsByType(GameAssetType.Jpeg));
        assets.AddRange(context.GetAssetsByType(GameAssetType.Png));

        this.Info("Acquired all other assets");

        ConcurrentQueue<GameAsset> assetQueue = new();
        foreach (GameAsset asset in assets) 
            assetQueue.Enqueue((GameAsset)asset.Clone(false));
        
        this.Info("Cloned Realm objects");

        int threadCount = Environment.ProcessorCount;

        List<Thread> threads = new(threadCount);

        for (int i = 0; i < threadCount; i++)
        {
            void Start() => this.ThreadTask(assetQueue, dataStore);
            Thread thread = new(Start);
            thread.Start();
            threads.Add(thread);
        }

        while (this._runningCount != 0)
        {
            Thread.Sleep(1);
        }

        this.Info($"Imported {assets.Count} images using {threadCount} threads in ~{this.Stopwatch.ElapsedMilliseconds}ms");
    }

    private int _runningCount;

    private void ThreadTask(ConcurrentQueue<GameAsset> assetQueue, IDataStore dataStore)
    {
        this._runningCount++;
        
        while (assetQueue.TryDequeue(out GameAsset? asset))
        {
            ImportAsset(asset, dataStore);
            this.Info($"Imported {asset.AssetType} {asset.AssetHash}");   
        }

        this._runningCount--;
    }

    public static void ImportAsset(GameAsset asset, IDataStore dataStore)
    {
        Stream stream = dataStore.GetStreamFromStore(asset.AssetHash);
        Stream writeStream = dataStore.OpenWriteStream("png/" + asset.AssetHash);

        switch (asset.AssetType)
        {
            case GameAssetType.Texture:
                TextureToPng(stream, writeStream);
                break;
            case GameAssetType.Jpeg:
                JpegToPng(stream, writeStream);
                break;
            case GameAssetType.Png:
                stream.CopyTo(writeStream); // TODO: use hard links instead of just replicating same data, or run 'optipng'?
                break;
            default:
                throw new InvalidOperationException($"Cannot convert a {asset.AssetType} to PNG");
        }
    }
}