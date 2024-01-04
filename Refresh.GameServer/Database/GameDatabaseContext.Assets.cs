using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // AssetConfiguration
{
    public GameAsset? GetAssetFromHash(string hash) =>
        this._realm.All<GameAsset>()
            .FirstOrDefault(a => a.AssetHash == hash);

    public GameAssetType? GetConvertedType(string hash)
    {
        IQueryable<GameAsset> assets = this._realm.All<GameAsset>();
        
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
        this._realm.All<GameAsset>()
            .Where(a => a._AssetType == (int)type);

    public void AddAssetToDatabase(GameAsset asset) =>
        this._realm.Write(() =>
        {
            this._realm.Add(asset);
        });

    public void AddOrUpdateAssetsInDatabase(IEnumerable<GameAsset> assets) =>
        this._realm.Write(() =>
        {
            this._realm.Add(assets, true);
        });

    public void SetAsMainlineIconHash(GameAsset asset, string hash) =>
        this._realm.Write(() =>
        {
            asset.AsMainlineIconHash = hash;
        });
    
    public void SetAsMipIconHash(GameAsset asset, string hash) =>
        this._realm.Write(() =>
        {
            asset.AsMipIconHash = hash;
        });
    
    public void SetAsMainlinePhotoHash(GameAsset asset, string hash) =>
        this._realm.Write(() =>
        {
            asset.AsMainlinePhotoHash = hash;
        });
}