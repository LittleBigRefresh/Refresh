using MongoDB.Bson;
using Bunkum.Core.RateLimit;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Statistics;

namespace Refresh.Database.Models.Users;

[JsonObject(MemberSerialization.OptIn)]
[Index(nameof(Username), nameof(UsernameLower), nameof(EmailAddress), nameof(PasswordBcrypt))]
public partial class GameUser : IRateLimitUser
{
    [Key] public ObjectId UserId { get; set; } = ObjectId.GenerateNewId();
    public string Username { get; set; } = string.Empty;
    public string? EmailAddress { get; set; }
    public string? PasswordBcrypt { get; set; } = null;
    
    public bool EmailAddressVerified { get; set; }
    public bool ShouldResetPassword { get; set; }
    
    public string IconHash { get; set; } = "0";
    
    public GameUserStatistics? Statistics { get; set; }

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

    #region Timed Level Limit

    /// <summary>
    /// How many levels the user published/overwrote during the configured timed level limit,
    /// if enabled.
    /// </summary>
    public int TimedLevelUploads { get; set; }
    /// <summary>
    /// The timestamp when this user's timed level limit will reset.
    /// When that happens, set this property to null and reset TimedLevelUploads to 0.
    /// </summary>
    public DateTimeOffset? TimedLevelUploadExpiryDate { get; set; }

    #endregion

    public string Description { get; set; } = "";

    public int LocationX { get; set; }
    public int LocationY { get; set; }
    
    public DateTimeOffset JoinDate { get; set; }

    public string BetaPlanetsHash { get; set; } = "0";
    public string Lbp2PlanetsHash { get; set; } = "0";
    public string Lbp3PlanetsHash { get; set; } = "0";
    public string VitaPlanetsHash { get; set; } = "0";

    public string YayFaceHash { get; set; } = "0";
    public string BooFaceHash { get; set; } = "0";
    public string MehFaceHash { get; set; } = "0";

    public bool AllowIpAuthentication { get; set; }
    
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

    /// <summary>
    /// Whether the user's profile information is exposed in the public API.
    /// </summary>
    public Visibility ProfileVisibility { get; set; } = Visibility.All;

    /// <summary>
    /// Whether the user's levels are exposed in the public API.
    /// </summary>
    public Visibility LevelVisibility { get; set; } = Visibility.All;

    /// <summary>
    /// If `true`, unescape XML tags sent to /filter
    /// </summary>
    public bool UnescapeXmlSequences { get; set; }
    
    public GameUserRole Role { get; set; }

    /// <summary>
    /// Whether modded content should be shown in level listings
    /// </summary>
    public bool ShowModdedContent { get; set; } = true;

    /// <summary>
    /// Whether reuploaded content should be shown in level listings
    /// </summary>
    public bool ShowReuploadedContent { get; set; } = true;

    public string UsernameLower
    {
        get => Username.ToLower();
        // ReSharper disable once ValueParameterNotUsed
        set {}
    }

    public override string ToString() => $"{this.Username} ({this.Role})";

    #region Rate-limiting
    public bool RateLimitUserIdIsEqual(object obj)
    {
        if (obj is not ObjectId id) return false;
        return this.UserId.Equals(id);
    }

    [NotMapped] public object RateLimitUserId => this.UserId; 

    #endregion

    [NotMapped] public bool FakeUser { get; set; } = false;
}