using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Assets
{
    public GameAsset? GetAssetFromHash(string hash)
    {
        return this._realm.All<GameAsset>().FirstOrDefault(a => a.AssetHash == hash);
    }

    public void AddAssetToDatabase(string hash, GameUser uploader)
    {
        GameAsset asset = new()
        {
            AssetHash = hash,
            AssetType = GameAssetType.Unknown,
            OriginalUploader = uploader,
            UploadDate = DateTimeOffset.Now,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(asset);
        });
    }

    public void AddAssetsToDatabase(IEnumerable<GameAsset> assets)
    {
        this._realm.Write(() =>
        {
            this._realm.Add(assets);
        });
    }

    public void DeleteAllAssetMetadata()
    {
        this._realm.Write(() =>
        {
            this._realm.RemoveAll<GameAsset>();
        });
    }
}