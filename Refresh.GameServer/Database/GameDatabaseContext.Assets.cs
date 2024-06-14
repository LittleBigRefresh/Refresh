using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // AssetConfiguration
{
    public GameAsset? GetAssetFromHash(string hash)
    {
        if (hash == "0" || hash.StartsWith('g')) return null;
        
        return this._realm.All<GameAsset>()
            .FirstOrDefault(a => a.AssetHash == hash);
    }
    
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

    public IEnumerable<string> GetAssetDependencies(GameAsset asset) 
        => this._realm.All<AssetDependencyRelation>().Where(a => a.Dependent == asset.AssetHash)
            .AsEnumerable()
            .Select(a => a.Dependency);

    public void AddAssetDependencyRelation(string dependent, string dependency)
    {
        this._realm.Write(() =>
        {
            this._realm.Add(new AssetDependencyRelation
            {
                Dependent = dependent,
                Dependency = dependency,
            }); 
        });
    }

    public void AddAssetDependencyRelations(string dependent, IEnumerable<string> dependencies)
    {
        this._realm.Write(() =>
        {
            foreach (string dependency in dependencies)
                this._realm.Add(new AssetDependencyRelation
                {
                    Dependent = dependent,
                    Dependency = dependency,
                });
        });
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

    public void SetMainlineIconHash(GameAsset asset, string hash) =>
        this._realm.Write(() =>
        {
            asset.AsMainlineIconHash = hash;
        });
    
    public void SetMipIconHash(GameAsset asset, string hash) =>
        this._realm.Write(() =>
        {
            asset.AsMipIconHash = hash;
        });
    
    public void SetMainlinePhotoHash(GameAsset asset, string hash) =>
        this._realm.Write(() =>
        {
            asset.AsMainlinePhotoHash = hash;
        });
}