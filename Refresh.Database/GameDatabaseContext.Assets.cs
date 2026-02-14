using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Relations;
namespace Refresh.Database;

public partial class GameDatabaseContext // Assets
{
    public IQueryable<GameAsset> GameAssetsIncluded => this.GameAssets
        .Include(a => a.OriginalUploader);
    
    public GameAsset? GetAssetFromHash(string hash)
    {
        if (hash.IsBlankHash() || hash.StartsWith('g')) return null;
        
        return this.GameAssetsIncluded
            .FirstOrDefault(a => a.AssetHash == hash);
    }

    public IQueryable<GameAsset> GetAssetsUploadedByUserInternal(GameUser? user)
        => this.GameAssetsIncluded.Where(a => a.OriginalUploader == user);

    public DatabaseList<GameAsset> GetAssetsUploadedByUser(GameUser? user, int skip, int count)
        => new(this.GetAssetsUploadedByUserInternal(user), skip, count);
    
    public DatabaseList<GameAsset> GetAssetsUploadedByUser(GameUser? user, int skip, int count, GameAssetType type)
        => new(this.GetAssetsUploadedByUserInternal(user).Where(a => a.AssetType == type), skip, count);

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

    // TODO: optimize this by returning a list of GameAsset instead
    public IEnumerable<string> GetAssetDependencies(GameAsset asset) 
        => this.AssetDependencyRelations.Where(a => a.Dependent == asset.AssetHash)
            .AsEnumerable()
            .Select(a => a.Dependency);
    
    // TODO: optimize this by returning a list of GameAsset instead
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
        this.GameAssetsIncluded
            .Where(a => a.AssetType == type);

    public void AddAssetToDatabase(GameAsset asset) =>
        this.Write(() =>
        {
            this.GameAssets.Add(asset);
        });
    
    public void AddAssetsToDatabase(IEnumerable<GameAsset> assets)
    {
        this.GameAssets.AddRange(assets);
        this.SaveChanges();
    }
    
    public void UpdateAssetsInDatabase(IEnumerable<GameAsset> assets)
    {
        this.GameAssets.UpdateRange(assets);
        this.SaveChanges();
    }

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