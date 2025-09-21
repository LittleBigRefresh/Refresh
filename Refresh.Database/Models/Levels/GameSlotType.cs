using System.Xml.Serialization;

namespace Refresh.Database.Models.Levels;

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
    /// <summary>
    /// A moon level.
    /// </summary>
    [XmlEnum("local")]
    Moon,
    /// <summary>
    /// The pod.
    /// </summary>
    [XmlEnum("pod")]
    Pod,
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
            GameSlotType.Moon => "local",
            GameSlotType.Pod => "pod",
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null),
        };
    }
}