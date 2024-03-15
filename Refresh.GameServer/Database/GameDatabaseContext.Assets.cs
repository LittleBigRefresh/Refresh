using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // AssetConfiguration
{
    public GameAsset? GetAssetFromHash(string hash) =>
        this.All<GameAsset>()
            .FirstOrDefault(a => a.AssetHash == hash);

    public GameAssetType? GetConvertedType(string hash)
    {
        IQueryable<GameAsset> assets = this.All<GameAsset>();
        
        foreach (GameAsset asset in assets)
        {
            //If the asset hash is the requested hash, its not a converted file
            if (asset.AssetHash == hash) return null;
            
            if (asset.AsMainlineIconHash == hash)
                return GameAssetType.Png;

            if (asset.AsMipIconHash == hash)
                return GameAssetType.Mip;
        }
        
        return null;
    }
    
    public IEnumerable<GameAsset> GetAssetsByType(GameAssetType type) =>
        this.All<GameAsset>()
            .Where(a => a._AssetType == (int)type);

    public void AddAssetToDatabase(GameAsset asset) =>
        this.Write(() =>
        {
            this.Add(asset);
        });

    public void AddOrUpdateAssetsInDatabase(IEnumerable<GameAsset> assets) =>
        this.Write(() =>
        {
            this.AddRange(assets, true);
        });

    public void SetMainlineIconHash(GameAsset asset, string hash) =>
        this.Write(() =>
        {
            asset.AsMainlineIconHash = hash;
        });
    
    public void SetMipIconHash(GameAsset asset, string hash) =>
        this.Write(() =>
        {
            asset.AsMipIconHash = hash;
        });
    
    public void SetMainlinePhotoHash(GameAsset asset, string hash) =>
        this.Write(() =>
        {
            asset.AsMainlinePhotoHash = hash;
        });
}