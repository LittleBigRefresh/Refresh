using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Extensions;

public static class GameDatabaseContextExtensions
{
    public static void UpdatePlanetModdedStatus(this GameDatabaseContext database, GameUser user)
    {
        user.AreLbp2PlanetsModded = database.GetPlanetModdedStatus(user.Lbp2PlanetsHash);
        user.AreLbp3PlanetsModded = database.GetPlanetModdedStatus(user.Lbp3PlanetsHash);
        user.AreVitaPlanetsModded = database.GetPlanetModdedStatus(user.VitaPlanetsHash);
        user.AreBetaPlanetsModded = database.GetPlanetModdedStatus(user.BetaPlanetsHash);
    }

    public static bool GetPlanetModdedStatus(this GameDatabaseContext database, string rootAssetHash)
    {
        bool modded = false;

        GameAsset? rootAsset = database.GetAssetFromHash(rootAssetHash);
        rootAsset?.TraverseDependenciesRecursively(database, (_, asset) =>
        {
            if (asset != null && (asset.AssetFlags & (AssetFlags.Modded | AssetFlags.ModdedOnPlanets)) != 0)
                modded = true;
        });
        
        return modded;
    }
}