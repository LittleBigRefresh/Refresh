using System.Xml.Serialization;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.Game.Handshake;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Types;

/// <summary>
/// Expected visibility settings for client
/// </summary>
/// <seealso cref="MetadataEndpoints"/>
public enum Visibility
{
    /// <summary>
    /// User is okay with content being shown everywhere at all times
    /// </summary>
    [XmlEnum("all")]
    All = 0,
    /// <summary>
    /// User only allows content to be shown in-game and on the website to authenticated viewers 
    /// </summary>
    [XmlEnum("psn")] // Yes it says PSN, but in-game it is described as "users who are logged into PSN on the website"
    LoggedInUsers = 1,
    /// <summary>
    /// User only allows content to be shown in-game
    /// </summary>
    [XmlEnum("game")]
    Game = 2,
}

public static class VisibilityExtensions
{
    public static T? Filter<T>(this Visibility visibility, DataContext dataContext, T? obj) where T : class
    {
        switch (visibility)
        {
            case Visibility.Game when dataContext.Game != TokenGame.Website:
            case Visibility.LoggedInUsers when dataContext.Token != null:
            case Visibility.All:
                return obj;
            default:
                return null;
        }
    }
}