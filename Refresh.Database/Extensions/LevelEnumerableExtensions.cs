using Refresh.Database.Query;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database.Extensions;

public static class LevelEnumerableExtensions
{
    public static IEnumerable<GameLevel> FilterByGameVersion(this IEnumerable<GameLevel> levels, TokenGame gameVersion)
        => gameVersion switch
        {
            TokenGame.LittleBigPlanet1 => levels.Where(l => l.GameVersion == TokenGame.LittleBigPlanet1),
            TokenGame.LittleBigPlanet2 => levels.Where(l => l.GameVersion <= TokenGame.LittleBigPlanet2),
            TokenGame.LittleBigPlanet3 => levels.Where(l => l.GameVersion <= TokenGame.LittleBigPlanet3),
            TokenGame.LittleBigPlanetVita => levels.Where(l => l.GameVersion == TokenGame.LittleBigPlanetVita),
            TokenGame.LittleBigPlanetPSP => levels.Where(l => l.GameVersion == TokenGame.LittleBigPlanetPSP),
            TokenGame.BetaBuild => levels.Where(l => l.GameVersion == TokenGame.BetaBuild),
            TokenGame.Website => levels,
            _ => throw new ArgumentOutOfRangeException(nameof(gameVersion), gameVersion, null),
        };

    public static IQueryable<GameLevel> FilterByGameVersion(this IQueryable<GameLevel> levels, TokenGame gameVersion)
        => gameVersion switch
        {
            TokenGame.LittleBigPlanet1 => levels.Where(l => l.GameVersion == TokenGame.LittleBigPlanet1),
            TokenGame.LittleBigPlanet2 => levels.Where(l => l.GameVersion <= TokenGame.LittleBigPlanet2),
            TokenGame.LittleBigPlanet3 => levels.Where(l => l.GameVersion <= TokenGame.LittleBigPlanet3),
            TokenGame.LittleBigPlanetVita => levels.Where(l => l.GameVersion == TokenGame.LittleBigPlanetVita),
            TokenGame.LittleBigPlanetPSP => levels.Where(l => l.GameVersion == TokenGame.LittleBigPlanetPSP),
            TokenGame.BetaBuild => levels.Where(l => l.GameVersion == TokenGame.BetaBuild),
            TokenGame.Website => levels,
            _ => throw new ArgumentOutOfRangeException(nameof(gameVersion), gameVersion, null),
        };

    public static IQueryable<GameLevel> FilterByLevelFilterSettings(this IQueryable<GameLevel> levels, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        if (levelFilterSettings.ExcludeMyLevels && user != null)
            levels = levels.Where(l => l.Publisher != null && l.Publisher.UserId != user.UserId);

        // If the user has it disabled globally and the filter doesnt enable it, or the filter disables it, disable modded content from being shown
        if ((user is { ShowModdedContent: false } && levelFilterSettings.ShowModdedLevels != true) || levelFilterSettings.ShowModdedLevels == false)
            levels = levels.Where(l => !l.IsModded);
        
        if ((user is { ShowReuploadedContent: false } && levelFilterSettings.ShowReuploadedLevels != true) || levelFilterSettings.ShowReuploadedLevels == false)
            levels = levels.Where(l => !l.IsReUpload);

        if (levelFilterSettings.Players != 0) 
            levels = levels.Where(l => l.MaxPlayers >= levelFilterSettings.Players && l.MinPlayers <= levelFilterSettings.Players);

        if (!levelFilterSettings.DisplayLbp1) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanet1);
        if (!levelFilterSettings.DisplayLbp2) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanet2);
        if (!levelFilterSettings.DisplayLbp3) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanet3);
        if (!levelFilterSettings.DisplayVita) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanetVita);
        if (!levelFilterSettings.DisplayPSP) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanetPSP);
        if (!levelFilterSettings.DisplayBeta) levels = levels.Where(l => l.GameVersion != TokenGame.BetaBuild);
        
        switch(levelFilterSettings.DisplayMoveLevels)
        {
            case PropertyFilterType.Include:
                // Do nothing
                break;
            case PropertyFilterType.Only:
                levels = levels.Where(l => l.RequiresMoveController == true);
                break;
            case PropertyFilterType.Exclude:
                levels = levels.Where(l => l.RequiresMoveController == false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(levelFilterSettings.DisplayMoveLevels), levelFilterSettings.DisplayMoveLevels, "Unsupported value");
        }
        
        // Filter out sub levels that weren't published by self
        levels = levels.Where(l => !l.IsSubLevel || l.Publisher == user);

        return levels;
    }
    
    public static IEnumerable<GameLevel> FilterByLevelFilterSettings(this IEnumerable<GameLevel> levels, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        if (levelFilterSettings.ExcludeMyLevels && user != null)
            levels = levels.Where(l => l.Publisher != null && l.Publisher.UserId != user.UserId);

        // If the user has it disabled globally and the filter doesnt enable it, or the filter disables it, disable modded content from being shown
        if ((user is { ShowModdedContent: false } && levelFilterSettings.ShowModdedLevels != true) || levelFilterSettings.ShowModdedLevels == false)
            levels = levels.Where(l => !l.IsModded);
        
        if ((user is { ShowReuploadedContent: false } && levelFilterSettings.ShowReuploadedLevels != true) || levelFilterSettings.ShowReuploadedLevels == false)
            levels = levels.Where(l => !l.IsReUpload);

        if (levelFilterSettings.Players != 0) 
            levels = levels.Where(l => l.MaxPlayers >= levelFilterSettings.Players && l.MinPlayers <= levelFilterSettings.Players);

        if (!levelFilterSettings.DisplayLbp1) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanet1);
        if (!levelFilterSettings.DisplayLbp2) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanet2);
        if (!levelFilterSettings.DisplayLbp3) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanet3);
        if (!levelFilterSettings.DisplayVita) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanetVita);
        if (!levelFilterSettings.DisplayPSP) levels = levels.Where(l => l.GameVersion != TokenGame.LittleBigPlanetPSP);
        if (!levelFilterSettings.DisplayBeta) levels = levels.Where(l => l.GameVersion != TokenGame.BetaBuild);
        
        switch(levelFilterSettings.DisplayMoveLevels)
        {
            case PropertyFilterType.Include:
                // Do nothing
                break;
            case PropertyFilterType.Only:
                levels = levels.Where(l => l.RequiresMoveController == true);
                break;
            case PropertyFilterType.Exclude:
                levels = levels.Where(l => l.RequiresMoveController == false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(levelFilterSettings.DisplayMoveLevels), levelFilterSettings.DisplayMoveLevels, "Unsupported value");
        }
        
        // Filter out sub levels that weren't published by self
        levels = levels.Where(l => !l.IsSubLevel || l.Publisher == user);

        return levels;
    }
}