namespace Refresh.GameServer.Types.Levels;

public enum GameLevelSource
{
    /// <summary>
    /// A level uploaded by a user to the server
    /// </summary>
    User,
    /// <summary>
    /// A level created by the server to represent a game story level.
    /// </summary>
    Story,
}