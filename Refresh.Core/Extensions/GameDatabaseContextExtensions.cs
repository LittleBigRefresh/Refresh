using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;

namespace Refresh.Core.Extensions;

public static class GameDatabaseContextExtensions
{
    public static void UpdateLevelModdedStatus(this GameDatabaseContext database, GameLevel level, bool save = true)
    {
        database.SetLevelModdedStatus(level, database.GetLevelModdedStatus(level), save);
    }
    
    public static bool GetLevelModdedStatus(this GameDatabaseContext database, GameLevel level)
    {
        // Skip this for PSP assets, as we can't read them nor determine their type (yet)
        if (level.GameVersion == TokenGame.LittleBigPlanetPSP)
            return false;

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