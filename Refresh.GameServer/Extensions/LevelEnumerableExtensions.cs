using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Extensions;

public static class LevelEnumerableExtensions
{
    public static IEnumerable<GameLevel> FilterByGameVersion(this IEnumerable<GameLevel> levels, TokenGame gameVersion)
        => gameVersion switch
        {
            TokenGame.LittleBigPlanet1 => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanet1),
            TokenGame.LittleBigPlanet2 => levels.Where(l => l._GameVersion <= (int)TokenGame.LittleBigPlanet2),
            TokenGame.LittleBigPlanet3 => levels.Where(l => l._GameVersion <= (int)TokenGame.LittleBigPlanet3),
            TokenGame.LittleBigPlanetVita => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanetVita),
            TokenGame.LittleBigPlanetPSP => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanetPSP),
            TokenGame.BetaBuild => levels.Where(l => l._GameVersion == (int)TokenGame.BetaBuild),
            TokenGame.Website => levels,
            _ => throw new ArgumentOutOfRangeException(nameof(gameVersion), gameVersion, null),
        };

    public static IQueryable<GameLevel> FilterByGameVersion(this IQueryable<GameLevel> levels, TokenGame gameVersion)
        => gameVersion switch
        {
            TokenGame.LittleBigPlanet1 => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanet1),
            TokenGame.LittleBigPlanet2 => levels.Where(l => l._GameVersion <= (int)TokenGame.LittleBigPlanet2),
            TokenGame.LittleBigPlanet3 => levels.Where(l => l._GameVersion <= (int)TokenGame.LittleBigPlanet3),
            TokenGame.LittleBigPlanetVita => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanetVita),
            TokenGame.LittleBigPlanetPSP => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanetPSP),
            TokenGame.BetaBuild => levels.Where(l => l._GameVersion == (int)TokenGame.BetaBuild),
            TokenGame.Website => levels,
            _ => throw new ArgumentOutOfRangeException(nameof(gameVersion), gameVersion, null),
        };

    public static IQueryable<GameLevel> FilterByLevelFilterSettings(this IQueryable<GameLevel> levels, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        if (levelFilterSettings.ExcludeMyLevels && user != null)
            levels = levels.Where(l => l.Publisher != user);

        // If the user has it disabled globally and the filter doesnt enable it, or the filter disables it, disable modded content from being shown
        if ((user is { ShowModdedContent: false } && levelFilterSettings.ShowModdedLevels != true) || levelFilterSettings.ShowModdedLevels == false)
            levels = levels.Where(l => !l.IsModded);

        // Don't allow beta builds to use this filtering option
        // If the client specifies this option then it will filter out *all* levels.
        if (levelFilterSettings.GameVersion != TokenGame.BetaBuild)
        {
            levels = levelFilterSettings.GameFilterType switch {
                GameFilterType.LittleBigPlanet1 => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanet1),
                GameFilterType.LittleBigPlanet2 => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanet2),
                //NOTE: ideally this should be .Where(l => l._GameVersion == (int)TokenGame.LittleBigPlane1 || l._GameVersion == (int)TokenGame.LittleBigPlane2)
                //      however, there should be no differences in all real-world cases
                GameFilterType.Both => levels,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        if (levelFilterSettings.Players != 0) 
            levels = levels.Where(l => l.MaxPlayers >= levelFilterSettings.Players && l.MinPlayers <= levelFilterSettings.Players);

        if (!levelFilterSettings.DisplayLbp1) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanet1);
        if (!levelFilterSettings.DisplayLbp2) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanet2);
        if (!levelFilterSettings.DisplayLbp3) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanet3);
        if (!levelFilterSettings.DisplayVita) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanetVita);
        if (!levelFilterSettings.DisplayPSP) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanetPSP);
        if (!levelFilterSettings.DisplayBeta) levels = levels.Where(l => l._GameVersion != (int)TokenGame.BetaBuild);
        
        //TODO: store move compatibility for levels
        // levels = levelFilterSettings.MoveFilterType switch {
        // MoveFilterType.True => levels,
        // MoveFilterType.Only => levels.Where(l => l.MoveCompatible),
        // MoveFilterType.False => levels.Where(l => !l.MoveCompatible),
        // _ => throw new ArgumentOutOfRangeException()
        // };
        
        // Filter out sub levels that weren't published by self
        levels = levels.Where(l => !l.IsSubLevel || l.Publisher == user);

        return levels;
    }
    
    public static IEnumerable<GameLevel> FilterByLevelFilterSettings(this IEnumerable<GameLevel> levels, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        if (levelFilterSettings.ExcludeMyLevels && user != null)
            levels = levels.Where(l => l.Publisher != user);

        // If the user has it disabled globally and the filter doesnt enable it, or the filter disables it, disable modded content from being shown
        if ((user is { ShowModdedContent: false } && levelFilterSettings.ShowModdedLevels != true) || levelFilterSettings.ShowModdedLevels == false)
            levels = levels.Where(l => !l.IsModded);
        
        // Don't allow beta builds to use this filtering option
        // If the client specifies this option then it will filter out *all* levels.
        if (levelFilterSettings.GameVersion != TokenGame.BetaBuild)
        {
            levels = levelFilterSettings.GameFilterType switch {
                GameFilterType.LittleBigPlanet1 => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanet1),
                GameFilterType.LittleBigPlanet2 => levels.Where(l => l._GameVersion == (int)TokenGame.LittleBigPlanet2),
                //NOTE: ideally this should be .Where(l => l._GameVersion == (int)TokenGame.LittleBigPlane1 || l._GameVersion == (int)TokenGame.LittleBigPlane2)
                //      however, there should be no differences in all real-world cases
                GameFilterType.Both => levels,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        if (levelFilterSettings.Players != 0) 
            levels = levels.Where(l => l.MaxPlayers >= levelFilterSettings.Players && l.MinPlayers <= levelFilterSettings.Players);

        if (!levelFilterSettings.DisplayLbp1) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanet1);
        if (!levelFilterSettings.DisplayLbp2) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanet2);
        if (!levelFilterSettings.DisplayLbp3) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanet3);
        if (!levelFilterSettings.DisplayVita) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanetVita);
        if (!levelFilterSettings.DisplayPSP) levels = levels.Where(l => l._GameVersion != (int)TokenGame.LittleBigPlanetPSP);
        if (!levelFilterSettings.DisplayBeta) levels = levels.Where(l => l._GameVersion != (int)TokenGame.BetaBuild);
        
        //TODO: store move compatibility for levels
        // levels = levelFilterSettings.MoveFilterType switch {
        // MoveFilterType.True => levels,
        // MoveFilterType.Only => levels.Where(l => l.MoveCompatible),
        // MoveFilterType.False => levels.Where(l => !l.MoveCompatible),
        // _ => throw new ArgumentOutOfRangeException()
        // };
        
        // Filter out sub levels that weren't published by self
        levels = levels.Where(l => !l.IsSubLevel || l.Publisher?.UserId ==  user?.UserId);

        return levels;
    }
}