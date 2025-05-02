using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Relations;
namespace Refresh.Database;

public partial class GameDatabaseContext // Assets
{
    public GameAsset? GetAssetFromHash(string hash)
    {
        if (hash == "0" || hash.StartsWith('g')) return null;
        
        return this.GameAssets
            .FirstOrDefault(a => a.AssetHash == hash);
    }

    public DatabaseList<GameAsset> GetAssetsUploadedByUser(GameUser? user, int skip, int count)
        => new(this.GameAssets.Where(a => a.OriginalUploader == user), skip, count);
    
    public DatabaseList<GameAsset> GetAssetsUploadedByUser(GameUser? user, int skip, int count, GameAssetType type)
        => new(this.GameAssets.Where(a => a.OriginalUploader == user && a._AssetType == (int)type), skip, count);

    public GameAssetType? GetConvertedType(string hash)
    {
        GameAsset? asset = this.GameAssets.FirstOrDefault(a => a.AssetHash == hash ||
                                                                           a.AsMainlineIconHash == hash ||
                                                                           a.AsMipIconHash == hash);
        //If the asset hash is the requested hash, its not a converted file
        if (asset == null || asset.AssetHash == hash)
            return null;
            
        if (asset.AsMainlineIconHash == hash)
            return GameAssetType.Png;

        if (asset.AsMipIconHash == hash)
            return GameAssetType.Mip;

        return null;
    }

    public IEnumerable<string> GetAssetDependencies(GameAsset asset) 
        => this.AssetDependencyRelations.Where(a => a.Dependent == asset.AssetHash)
            .AsEnumerable()
            .Select(a => a.Dependency);
    
    public IEnumerable<string> GetAssetDependents(GameAsset asset) 
        => this.AssetDependencyRelations.Where(a => a.Dependency == asset.AssetHash)
            .AsEnumerable()
            .Select(a => a.Dependent);

    public void AddOrOverwriteAssetDependencyRelations(string dependent, IEnumerable<string> dependencies)
    {
        this.Write(() =>
        {
            // delete all existing relations. ensures duplicates won't exist when reprocessing
            this.AssetDependencyRelations.RemoveRange(a => a.Dependent == dependent);
            
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