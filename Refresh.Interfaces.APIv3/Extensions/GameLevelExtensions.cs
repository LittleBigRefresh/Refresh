using Refresh.Core.Types.Data;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;

namespace Refresh.Interfaces.APIv3.Extensions;

public static class GameLevelExtensions
{
    public static string GetIconHash(this GameLevel level, DataContext dataContext)
    {
        string hash = dataContext.GetIconFromHash(level.IconHash);
        return level.GameVersion == TokenGame.LittleBigPlanetPSP
            ? "psp/" + hash
            : hash;
    }
}
