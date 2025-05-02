namespace Refresh.Database.Models.Users;

/// <summary>
/// The state of a user, defining their permissions.
/// </summary>
/// <remarks>
/// A user can only have one role at a time.
/// </remarks>
public enum GameUserRole : sbyte
{
    /// <summary>
    /// An administrator of the instance. This user has all permissions, including the ability to manage other administrators.
    /// </summary>
    Admin = 127,
    /// <summary>
    /// A user that doesn't have moderator powers, but may team pick levels, set re-upload data, and adjust the level's game version.
    /// </summary>
    Curator = 64,
    /// <summary>
    /// A user with special permissions. May upload assets when asset uploads are otherwise disabled.
    /// </summary>
    Trusted = 1,
    /// <summary>
    /// A standard user. Can play the game, log in, play levels, review them, etc.
    /// </summary>
    User = 0,
    /// <summary>
    /// A user with read-only permissions. May log in and play, but cannot do things such as publish levels or post comments.
    /// </summary>
    Restricted = -126,
    /// <summary>
    /// A user that has been banned. Cannot log in, or do anything.
    /// </summary>
    Banned = -127,
}