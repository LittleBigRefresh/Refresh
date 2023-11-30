using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // AssetConfiguration
{
    public GameAsset? GetAssetFromHash(string hash) =>
        this.All<GameAsset>()
            .FirstOrDefault(a => a.AssetHash == hash);

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
}