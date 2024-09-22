using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Bunkum.Core.RateLimit;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Types.UserData;

[JsonObject(MemberSerialization.OptIn)]
public partial class GameUser : IRealmObject, IRateLimitUser
{
    [PrimaryKey] public ObjectId UserId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] public string Username { get; set; } = string.Empty;
    [Indexed] public string? EmailAddress { get; set; }
    [Indexed] public string? PasswordBcrypt { get; set; } = null;
    
    public bool EmailAddressVerified { get; set; }
    public bool ShouldResetPassword { get; set; }
    
    public string IconHash { get; set; } = "0";

    /// <summary>
    /// The force match of the user, cleared on login
    /// </summary>
    public ObjectId? ForceMatch { get; set; }
    
    /// <summary>
    /// The <see cref="IconHash"/>, except only for PSP clients.
    /// </summary>
    /// <remarks>
    /// PSP doesn't support remote assets, and instead only uses it's own GUID icon hashes, which cant be loaded by other clients.
    /// Hopefully this explains why this distinction is necessary.
    /// </remarks>
    public string PspIconHash { get; set; } = "0";
    /// <summary>
    /// The <see cref="IconHash"/>, except only for Vita clients.
    /// </summary>
    /// <remarks>
    /// Vita GUIDs do not map to mainline GUIDs, so we dont want someone to set their Vita icon, and it map to an invalid GUID on PS3.
    /// </remarks>
    public string VitaIconHash { get; set; } = "0";
    /// <summary>
    /// The <see cref="IconHash"/>, except only for clients in beta mode.
    /// </summary>
    public string BetaIconHash { get; set; } = "0";
    
    /// <summary>
    /// The cumulative size of all the assets the user has uploaded
    /// </summary>
    public int FilesizeQuotaUsage { get; set; }

    public string Description { get; set; } = "";

    public int LocationX { get; set; }
    public int LocationY { get; set; }
    
    public DateTimeOffset JoinDate { get; set; }
    public UserPins Pins { get; set; } = new();

    public string BetaPlanetsHash { get; set; } = "0";
    public string Lbp2PlanetsHash { get; set; } = "0";
    public string Lbp3PlanetsHash { get; set; } = "0";
    public string VitaPlanetsHash { get; set; } = "0";

    public string YayFaceHash { get; set; } = "0";
    public string BooFaceHash { get; set; } = "0";
    public string MehFaceHash { get; set; } = "0";

    public bool AllowIpAuthentication { get; set; }
    public string? CurrentVerifiedIp { get; set; }
    
    public string? BanReason { get; set; }
    public DateTimeOffset? BanExpiryDate { get; set; }
    
    public DateTimeOffset LastLoginDate { get; set; }
    
    public bool RpcnAuthenticationAllowed { get; set; }
    public bool PsnAuthenticationAllowed { get; set; }
    
    /// <summary>
    /// The auth token the presence server knows this user by, null if not connected to the presence server
    /// </summary>
    public string? PresenceServerAuthToken { get; set; }
    
    /// <summary>
    /// The user's root playlist. This playlist contains all the user's playlists, and optionally other slots as well,
    /// although the game does not expose the ability to do this normally.
    /// </summary>
    public GamePlaylist? RootPlaylist { get; set; }
    
    private int _ProfileVisibility { get; set; } = (int)Visibility.All;
    private int _LevelVisibility { get; set; } = (int)Visibility.All;
    private int _DiscordProfileVisibility { get; set; } = (int)Visibility.LoggedInUsers;
    private int _GitHubProfileVisibility { get; set; } = (int)Visibility.LoggedInUsers;
    
    /// <summary>
    /// Whether the user's profile information is exposed in the public API.
    /// </summary>
    [Ignored]
    public Visibility ProfileVisibility
    {
        get => (Visibility)this._ProfileVisibility;
        set => this._ProfileVisibility = (int)value;
    }
    
    /// <summary>
    /// Whether the user's levels are exposed in the public API.
    /// </summary>
    [Ignored]
    public Visibility LevelVisibility
    {
        get => (Visibility)this._LevelVisibility;
        set => this._LevelVisibility = (int)value;
    }
    
    /// <summary>
    /// Whether the user's discord profile is exposed in the public API
    /// </summary>
    [Ignored]
    public Visibility DiscordProfileVisibility
    {
        get => (Visibility)this._DiscordProfileVisibility;
        set => this._DiscordProfileVisibility = (int)value;
    }

    /// <summary>
    /// Whether the user's discord profile is exposed in the public API
    /// </summary>
    [Ignored]
    public Visibility GitHubProfileVisibility
    {
        get => (Visibility)this._GitHubProfileVisibility;
        set => this._GitHubProfileVisibility = (int)value;
    }
    
    /// <summary>
    /// If `true`, unescape XML tags sent to /filter
    /// </summary>
    public bool UnescapeXmlSequences { get; set; }
    
    [Ignored] public GameUserRole Role
    {
        get => (GameUserRole)this._Role;
        set => this._Role = (byte)value;
    }

    /// <summary>
    /// Whether or not modded content should be shown in level listings
    /// </summary>
    public bool ShowModdedContent { get; set; } = true;

    // ReSharper disable once InconsistentNaming
    internal byte _Role { get; set; }

    public override string ToString() => $"{this.Username} ({this.Role})";

    #region Rate-limiting
    public bool RateLimitUserIdIsEqual(object obj)
    {
        if (obj is not ObjectId id) return false;
        return this.UserId.Equals(id);
    }

    // Defined in authentication provider. Avoids Realm threading nonsense.
    [Ignored] [XmlIgnore] public object RateLimitUserId { get; internal set; } = null!;

    #endregion

    [Ignored] public bool FakeUser { get; set; } = false;
}