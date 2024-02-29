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

public static class GameLevelSourceExtensions
{
    public static string ToGameType(this GameLevelSource source)
    {
        return source switch
        {
            GameLevelSource.User => "user",
            GameLevelSource.Story => "developer",
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null),
        };
    }
}