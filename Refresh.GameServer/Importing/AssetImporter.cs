using System.Security.Cryptography;
using Bunkum.Core.Storage;
using JetBrains.Annotations;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Importing;

public class AssetImporter : Importer
{
    private readonly IDateTimeProvider _timeProvider;
    
    public AssetImporter(Logger? logger = null, IDateTimeProvider? timeProvider = null) : base(logger)
    {
        timeProvider ??= new SystemDateTimeProvider();
        this._timeProvider = timeProvider;
    }

    public void ImportFromDataStore(GameDatabaseContext database, IDataStore dataStore)
    {
        int updatedAssets = 0;
        int newAssets = 0;
        this.Stopwatch.Start();
        
        IEnumerable<string> assetHashes = dataStore.GetKeysFromStore();

        List<GameAsset> assets = new(assetHashes.Count());
        foreach (string path in assetHashes)
        {
            bool isPsp = path.StartsWith("psp/");
            //If the hash has a `/` and it doesnt start with `psp/`, then its an invalid asset
            if (path.Contains('/') && !isPsp) continue;

            string hash = isPsp ? path[4..] : path;
            
            byte[] data = dataStore.GetDataFromStore(path);
            
            GameAsset? newAsset = this.ReadAndVerifyAsset(hash, data, isPsp ? TokenPlatform.PSP : null);
            if (newAsset == null) continue;

            GameAsset? oldAsset = database.GetAssetFromHash(hash);

            if (oldAsset != null)
            {
                newAsset.OriginalUploader = oldAsset.OriginalUploader;
                newAsset.UploadDate = oldAsset.UploadDate;
                updatedAssets++;
            }
            else
            {
                newAssets++;
            }

            assets.Add(newAsset);
            this.Info($"Processed {newAsset.AssetType} asset {hash} ({AssetSafetyLevelExtensions.FromAssetType(newAsset.AssetType, newAsset.AssetFormat)})");
        }
        
        database.AddOrUpdateAssetsInDatabase(assets);

        int hashCount = newAssets + updatedAssets;
        
        this.Info($"Successfully imported {assets.Count}/{hashCount} assets ({newAssets} new, {updatedAssets} updated) into database");
        if (assets.Count < hashCount)
        {
            this.Warn($"{hashCount - assets.Count} assets were not imported");
        }
    }
    
    public static string BytesToHexString(ReadOnlySpan<byte> data)
    {
        Span<char> hexChars = stackalloc char[data.Length * 2];

        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];
            hexChars[i * 2] = GetHexChar(b >> 4); // High bits
            hexChars[i * 2 + 1] = GetHexChar(b & 0x0F); // Low bits
        }

        return new string(hexChars);

        static char GetHexChar(int value)
        {
            return (char)(value < 10 ? '0' + value : 'a' + value - 10);
        }
    }


    [Pure]
    public GameAsset? ReadAndVerifyAsset(string hash, byte[] data, TokenPlatform? platform)
    {
        string checkedHash = BytesToHexString(SHA1.HashData(data));

        if (checkedHash != hash)
        {
            this.Warn($"{hash} is actually hashed as {checkedHash} - this asset is likely corrupt.");
            return null;
        }
        
        (GameAssetType assetType, GameAssetFormat assetFormat) = this.DetermineAssetType(data, platform);
        
        GameAsset asset = new()
        {
            UploadDate = this._timeProvider.Now,
            OriginalUploader = null,
            AssetHash = hash,
            AssetType = assetType,
            AssetFormat = assetFormat,
            IsPSP = platform == TokenPlatform.PSP,
            SizeInBytes = data.Length,
        };
        
        if (asset.AssetFormat.HasDependencyTree())
        {
            try
            {
                List<string> dependencies = this.ParseAssetDependencies(data);
                foreach (string dependency in dependencies)
                {
                    asset.Dependencies.Add(dependency);
                }
            }
            catch (Exception e)
            {
                this.Warn($"Could not parse dependency tree for {hash}: {e}");
            }
        }

        return asset;
    }

    // See toolkit's source code for this: https://github.com/ennuo/toolkit/blob/15342e1afca2d5ac1de49e207922099e7aacef86/lib/cwlib/src/main/java/cwlib/types/SerializedResource.java#L113
    private List<string> ParseAssetDependencies(byte[] data)
    {
        MemoryStream ms = new(data);
        BEBinaryReader reader = new(ms);
        
        // Skip magic
        ms.Seek(4, SeekOrigin.Begin);
        
        // Read the head revision of the asset 
        uint head = reader.ReadUInt32();
        
        // Dependency lists were only added in revision 0x109, so if we are less than that, then just skip trying to parse out the dependency tree
        if (head < 0x109) 
            return [];

        uint dependencyTableOffset = reader.ReadUInt32();

        ms.Seek(dependencyTableOffset, SeekOrigin.Begin);
        uint dependencyCount = reader.ReadUInt32();

        this.Debug($"Dependency table offset: {dependencyTableOffset}, count: {dependencyCount}");
        
        List<string> dependencies = new((int)dependencyCount);

        Span<byte> hashBuffer = stackalloc byte[20];
        for (int i = 0; i < dependencyCount; i++)
        {
            byte flags = reader.ReadByte();
            if ((flags & 0x1) != 0) // UGC/SHA1
            {
                ms.ReadExactly(hashBuffer);
                dependencies.Add(BytesToHexString(hashBuffer));
            }
            else if ((flags & 0x2) != 0) reader.ReadUInt32(); // Skip GUID
                
            reader.ReadUInt32();
        }

        return dependencies;
    }
}