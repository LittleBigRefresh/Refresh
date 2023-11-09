using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Extensions;

public static class GameAssetExtensions
{
    public static void TraverseDependenciesRecursively(this GameAsset asset, GameDatabaseContext database, Action<string, GameAsset?> callback)
    {
        callback(asset.AssetHash, asset);
        foreach (string internalAssetHash in asset.Dependencies)
        {
            GameAsset? internalAsset = database.GetAssetFromHash(internalAssetHash);
            callback(internalAssetHash, internalAsset);

            internalAsset?.TraverseDependenciesRecursively(database, callback);
        }
    }
}