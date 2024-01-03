using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // AssetConfiguration
{
    public GameAsset? GetAssetFromHash(string hash) =>
        this._realm.All<GameAsset>()
            .FirstOrDefault(a => a.AssetHash == hash);

    public GameAssetType? GetConvertedType(string hash)
    {
        IQueryable<GameAsset> assets =this._realm.All<GameAsset>();
        
        foreach (GameAsset asset in assets)
        {
            if (asset.AsPngIconHash == hash)
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

    public void SetAsPngIconHash(GameAsset asset, string hash) =>
        this._realm.Write(() =>
        {
            asset.AsPngIconHash = hash;
        });
}