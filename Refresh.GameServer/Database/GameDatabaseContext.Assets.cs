using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // AssetConfiguration
{
    public GameAsset? GetAssetFromHash(string hash)
    {
        if (hash == "0" || hash.StartsWith('g')) return null;
        
        return this.GameAssets
            .FirstOrDefault(a => a.AssetHash == hash);
    }
    
    public GameAssetType? GetConvertedType(string hash)
    {
        IQueryable<GameAsset> assets = this.GameAssets;
        
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
        => this.AssetDependencyRelations.Where(a => a.Dependent == asset.AssetHash)
            .AsEnumerable()
            .Select(a => a.Dependency);

    public void AddOrOverwriteAssetDependencyRelations(string dependent, IEnumerable<string> dependencies)
    {
        this.Write(() =>
        {
            // delete all existing relations. ensures duplicates won't exist when reprocessing
            this._realm.RemoveRange(this.AssetDependencyRelations.Where(a => a.Dependent == dependent));
            
            foreach (string dependency in dependencies)
                this.AssetDependencyRelations.Add(new AssetDependencyRelation
                {
                    Dependent = dependent,
                    Dependency = dependency,
                });
        });
    }
    
    public IEnumerable<GameAsset> GetAssetsByType(GameAssetType type) =>
        this.GameAssets
            .Where(a => a._AssetType == (int)type);

    public void AddAssetToDatabase(GameAsset asset) =>
        this.Write(() =>
        {
            this.GameAssets.Add(asset);
        });

    public void AddOrUpdateAssetsInDatabase(IEnumerable<GameAsset> assets) =>
        this.Write(() =>
        {
            this.GameAssets.AddRange(assets, true);
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