using System.Collections.Concurrent;
using System.Diagnostics;
using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Common.Helpers;
using Refresh.Core.Metrics;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;

namespace Refresh.Core.Importing;

public partial class ImageImporter : Importer
{
    public ImageImporter(Logger? logger = null) : base(logger)
    {}

    public void ImportFromDataStore(GameDatabaseContext context, IDataStore dataStore)
    {
        this.Stopwatch.Start();

        List<GameAsset> assets = new();

        assets.AddRange(context.GetAssetsByType(GameAssetType.Texture));
        assets.AddRange(context.GetAssetsByType(GameAssetType.GameDataTexture));
        assets.AddRange(context.GetAssetsByType(GameAssetType.Jpeg));
        assets.AddRange(context.GetAssetsByType(GameAssetType.Png));
        assets.AddRange(context.GetAssetsByType(GameAssetType.Mip));

        this.Info("Acquired all other assets");

        ConcurrentQueue<GameAsset> assetQueue = new();
        foreach (GameAsset asset in assets) 
            assetQueue.Enqueue(asset);
        
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
            try
            {
                this.Info($"Imported {asset.AssetType} {asset.AssetHash} ({assetQueue.Count} remaining)");
                this.ImportAsset(asset.AssetHash, asset.IsPSP, asset.AssetType, dataStore);
            }
            catch (Exception ex)
            {
                this.Warn($"Failed to import {asset.AssetType} {asset.AssetHash}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        this._runningCount--;
    }

    public void ImportAsset(string hash, bool isPsp, GameAssetType? type, IDataStore dataStore)
    {
        string dataStorePath = isPsp ? $"psp/{hash}" : hash;

        if (type == null)
        {
            using Stream typeStream = dataStore.GetStreamFromStore(dataStorePath);
            Span<byte> span = stackalloc byte[16];
            _ = typeStream.Read(span);

            (GameAssetType type, GameAssetFormat) assetType = this.DetermineAssetType(span, isPsp ? TokenPlatform.PSP : TokenPlatform.Website);
            type = assetType.type;
        }
        
        using Stream stream = dataStore.GetStreamFromStore(dataStorePath);
        using Stream writeStream = dataStore.OpenWriteStream($"png/{hash}");

        Stopwatch sw = Stopwatch.StartNew();

        switch (type)
        {
            case GameAssetType.GameDataTexture:
                GtfToPng(stream, writeStream);
                GameServerMetrics.RecordImageConversion(sw);
                break;
            case GameAssetType.Mip: {
                byte[] rawData = dataStore.GetDataFromStore(dataStorePath);
                byte[] data = ResourceHelper.PspDecrypt(rawData, PSPKey.Value);

                using MemoryStream dataStream = new(data);

                MipToPng(dataStream, writeStream);
                GameServerMetrics.RecordImageConversion(sw);
                break;
            }
            case GameAssetType.Texture:
                TextureToPng(stream, writeStream);
                GameServerMetrics.RecordImageConversion(sw);
                break;
            case GameAssetType.Tga:
            case GameAssetType.Jpeg:
                ImageToPng(stream, writeStream);
                GameServerMetrics.RecordImageConversion(sw);
                break;
            case GameAssetType.Png:
                stream.CopyTo(writeStream); // TODO: use hard links instead of just replicating same data, or run 'optipng'?
                break;
            default:
                throw new InvalidOperationException($"Cannot convert a {type} to PNG");
        }
    }
}