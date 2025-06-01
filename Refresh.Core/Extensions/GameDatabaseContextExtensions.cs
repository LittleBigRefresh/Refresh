using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Levels;

namespace Refresh.Core.Extensions;

public static class GameDatabaseContextExtensions
{
    public static void UpdateLevelModdedStatus(this GameDatabaseContext database, GameLevel level)
    {
        database.SetLevelModdedStatus(level, database.GetLevelModdedStatus(level));
    }
    
    public static bool GetLevelModdedStatus(this GameDatabaseContext database, GameLevel level)
    {
        bool modded = false;

        GameAsset? rootAsset = database.GetAssetFromHash(level.RootResource);
        
        rootAsset?.TraverseDependenciesRecursively(database, (_, asset) =>
        {
            if (asset != null && (asset.AssetFlags & AssetFlags.Modded) != 0)
                modded = true;
        });
        
        return modded;
    }
}