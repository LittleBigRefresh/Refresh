using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Levels;

public enum GameSlotType
{
    /// <summary>
    /// A level uploaded by a user to the server.
    /// </summary>
    [XmlEnum("user")]
    User,
    /// <summary>
    /// A level created by the original developers.
    /// </summary>
    [XmlEnum("developer")]
    Story,
    /// <summary>
    /// An LBP1 playlist, displayed as a polaroid.
    /// </summary>
    [XmlEnum("playlist")]
    Playlist,
}

public static class GameLevelSourceExtensions
{
    public static string ToGameType(this GameSlotType source)
    {
        return source switch
        {
            GameSlotType.User => "user",
            GameSlotType.Story => "developer",
            GameSlotType.Playlist => "playlist",
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null),
        };
    }
}