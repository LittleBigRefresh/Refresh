using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;

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
            TokenGame.Website => levels,
            _ => throw new ArgumentOutOfRangeException(nameof(gameVersion), gameVersion, null),
        };
}