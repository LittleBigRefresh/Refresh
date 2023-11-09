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
        
        IEnumerable<string> assetHashes = dataStore.GetKeysFromStore()
            .Where(key => !key.Contains('/'));

        List<GameAsset> assets = new();
        foreach (string hash in assetHashes)
        {
            byte[] data = dataStore.GetDataFromStore(hash);
            
            GameAsset? newAsset = this.ReadAndVerifyAsset(hash, data, null);
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
            this.Info($"Processed {newAsset.AssetType} asset {hash} ({AssetSafetyLevelExtensions.FromAssetType(newAsset.AssetType)})");
        }
        
        database.AddOrUpdateAssetsInDatabase(assets);

        int hashCount = assetHashes.Count();
        
        this.Info($"Successfully imported {assets.Count}/{hashCount} assets ({newAssets} new, {updatedAssets} updated) into database");
        if (assets.Count < hashCount)
        {
            this.Warn($"{hashCount - assets.Count} assets were not imported");
        }
    }

    [Pure]
    public GameAsset? ReadAndVerifyAsset(string hash, byte[] data, TokenPlatform? platform)
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
            UploadDate = this._timeProvider.Now,
            OriginalUploader = null,
            AssetHash = hash,
            AssetType = this.DetermineAssetType(data, platform),
            IsPSP = platform == TokenPlatform.PSP,
            SizeInBytes = data.Length,
        };

        return asset;
    }
}