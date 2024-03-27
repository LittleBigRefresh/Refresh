using Bunkum.Core.Configuration;

namespace Refresh.GameServer.Configuration;

/// <summary>
/// A configuration holding basic contact information for those operating this instance.
/// </summary>
public class ContactInfoConfig : Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {}

    /// <summary>
    /// The owner's screen name.
    /// </summary>
    public string AdminName { get; set; } = "Administrator";
    /// <summary>
    /// The owner's email address.
    /// </summary>
    public string EmailAddress { get; set; } = "refresh@example.com";
    /// <summary>
    /// A link to a Discord server.
    /// </summary>
    public string? DiscordServerInvite { get; set; }
    /// <summary>
    /// The owner's personal Discord account.
    /// </summary>
    public string? AdminDiscordUsername { get; set; }
}