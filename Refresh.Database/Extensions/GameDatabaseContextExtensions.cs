using Refresh.Database.Models.Assets;

namespace Refresh.Database.Extensions;

public static class GameDatabaseContextExtensions
{
    public static bool GetPlanetModdedStatus(this GameDatabaseContext database, string rootAssetHash)
    {
        bool modded = false;

        GameAsset? rootAsset = database.GetAssetFromHash(rootAssetHash);
        
        rootAsset?.TraverseDependenciesRecursively(database, (_, asset) =>
        {
            if (asset != null && (asset.AssetFlags & AssetFlags.Modded & AssetFlags.ModdedOnPlanets) != 0)
                modded = true;
        });
        
        return modded;
    }
}