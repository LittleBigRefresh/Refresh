namespace Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;

/// <summary>
/// What game to filter to, only relevant to LBP2
/// </summary>
public enum GameFilterType
{
    /// <summary>
    /// Include LBP1 levels
    /// </summary>
    LittleBigPlanet1,
    /// <summary>
    /// Include LBP2 levels
    /// </summary>
    LittleBigPlanet2,
    /// <summary>
    /// Include both LBP1 and LBP2 levels
    /// </summary>
    Both,
}
