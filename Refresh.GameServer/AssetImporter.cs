using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
    private readonly LoggerContainer<BunkumContext> _logger;
    private readonly Stopwatch _stopwatch;

    public AssetImporter(LoggerContainer<BunkumContext>? logger = null)
    {
        if (logger == null)
        {
            logger = new LoggerContainer<BunkumContext>();
            logger.RegisterLogger(new ConsoleLogger());
        }

        this._logger = logger;
        this._stopwatch = new Stopwatch();
    }

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

    public void ImportFromDataStore(GameDatabaseContext context, IDataStore dataStore)
    {
        this._stopwatch.Start();
        
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesMagic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> magic)
    {
        return data[..magic.Length].SequenceEqual(magic);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesMagic(ReadOnlySpan<byte> data, uint magic)
    {
        Span<byte> magicSpan = stackalloc byte[sizeof(uint)];
        BitConverter.TryWriteBytes(magicSpan, BinaryPrimitives.ReverseEndianness(magic));
        return MatchesMagic(data, magicSpan);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesMagic(ReadOnlySpan<byte> data, ulong magic)
    {
        Span<byte> magicSpan = stackalloc byte[sizeof(ulong)];
        BitConverter.TryWriteBytes(magicSpan, BinaryPrimitives.ReverseEndianness(magic));
        return MatchesMagic(data, magicSpan);
    }

    private static GameAssetType DetermineAssetType(ReadOnlySpan<byte> data)
    {
        // LBP assets
        if (MatchesMagic(data, "TEX "u8)) return GameAssetType.Texture;
        if (MatchesMagic(data, "PLNb"u8)) return GameAssetType.Plan;
        if (MatchesMagic(data, "LVLb"u8)) return GameAssetType.Level;
        
        // Traditional files
        // Good reference for magics: https://en.wikipedia.org/wiki/List_of_file_signatures
        if (MatchesMagic(data, 0xFFD8FFE0)) return GameAssetType.Jpeg;
        if (MatchesMagic(data, 0x89504E470D0A1A0A)) return GameAssetType.Png;

        return GameAssetType.Unknown;
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