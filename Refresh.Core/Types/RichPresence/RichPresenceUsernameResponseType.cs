namespace Refresh.GameServer.Types.RichPresence;

/// <summary>
/// Specifies how a user's link should be presented in rich presence.
/// </summary>
public enum RichPresenceUsernameResponseType : byte
{
    /// <summary>
    /// Generate URLs based on the user's ID.
    /// </summary>
    /// <remarks>
    /// For Lighthouse applications, this means the user's **Legacy** id, which Refresh does not support lookups for.
    /// </remarks>
    UserId = 0,
    /// <summary>
    /// Generate URLs based on the user's username.
    /// </summary>
    Username = 1,
}