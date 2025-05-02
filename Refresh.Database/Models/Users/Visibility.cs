using System.Xml.Serialization;

namespace Refresh.GameServer.Types;

/// <seealso cref="Refresh.GameServer.Endpoints.Game.Handshake.MetadataEndpoints"/>
public enum Visibility
{
    /// <summary>
    /// User is okay with content being shown everywhere
    /// </summary>
    [XmlEnum("all")]
    All = 0,
    /// <summary>
    /// User only allows content to be shown in-game and on website 
    /// </summary>
    [XmlEnum("psn")] // Yes it says PSN, but in-game it is described as "users who are logged into PSN on the website"
    Website = 1,
    /// <summary>
    /// User only allows content to be shown in-game
    /// </summary>
    [XmlEnum("game")]
    Game = 2,
}