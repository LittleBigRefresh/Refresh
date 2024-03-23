using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Comments;
using Bunkum.Core.RateLimit;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Relations;
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
    public GameLocation Location { get; set; } = GameLocation.Zero;
    
    public DateTimeOffset JoinDate { get; set; }
    public UserPins Pins { get; set; } = new();
    
    #nullable disable
    public IList<GameComment> ProfileComments { get; }
    
    [Backlink(nameof(FavouriteLevelRelation.User))]
    public IQueryable<FavouriteLevelRelation> FavouriteLevelRelations { get; }
    
    [Backlink(nameof(QueueLevelRelation.User))]
    public IQueryable<QueueLevelRelation> QueueLevelRelations { get; }
    
    [Backlink(nameof(FavouriteUserRelation.UserToFavourite))]
    public IQueryable<FavouriteUserRelation> UsersFavouritingMe { get; }
    
    [Backlink(nameof(FavouriteUserRelation.UserFavouriting))]
    public IQueryable<FavouriteUserRelation> UsersFavourited { get; }

    [Backlink(nameof(GameLevel.Publisher))]
    public IQueryable<GameLevel> PublishedLevels { get; }
    
    [Backlink(nameof(GamePhoto.Publisher))]
    public IQueryable<GamePhoto> PhotosByMe { get; }
    
    [Backlink(nameof(GamePhotoSubject.User))]
    public IQueryable<GamePhotoSubject> PhotosWithMe { get; }
    
    public IList<GameIpVerificationRequest> IpVerificationRequests { get; }
    #nullable restore

    public string BetaPlanetsHash { get; set; } = "0";
    public string Lbp2PlanetsHash { get; set; } = "0";
    public string Lbp3PlanetsHash { get; set; } = "0";
    public string VitaPlanetsHash { get; set; } = "0";

    public bool AllowIpAuthentication { get; set; }
    public string? CurrentVerifiedIp { get; set; }
    
    public string? BanReason { get; set; }
    public DateTimeOffset? BanExpiryDate { get; set; }
    
    public DateTimeOffset LastLoginDate { get; set; }
    
    public bool RpcnAuthenticationAllowed { get; set; }
    public bool PsnAuthenticationAllowed { get; set; }
    
    /// <summary>
    /// If `true`, turn all grief reports into photo uploads
    /// </summary>
    public bool RedirectGriefReportsToPhotos { get; set; }
    /// <summary>
    /// If `true`, unescape XML tags sent to /filter
    /// </summary>
    public bool UnescapeXmlSequences { get; set; }
    
    [Ignored] public GameUserRole Role
    {
        get => (GameUserRole)this._Role;
        set => this._Role = (byte)value;
    }

    // ReSharper disable once InconsistentNaming
    internal byte _Role { get; set; }

    public override string ToString() => $"{this.Username} ({this.UserId})";

    #region Rate-limiting
    public bool RateLimitUserIdIsEqual(object obj)
    {
        if (obj is not ObjectId id) return false;
        return this.UserId.Equals(id);
    }

    // Defined in authentication provider. Avoids Realm threading nonsense.
    [Ignored] [XmlIgnore] public object RateLimitUserId { get; internal set; } = null!;

    #endregion
}