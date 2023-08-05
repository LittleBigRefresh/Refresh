using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Comments;
using Bunkum.HttpServer.RateLimit;
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
    [Indexed] public string? PasswordBcrypt { get; set; } = null;
    public string IconHash { get; set; } = "0";

    public string Description { get; set; } = "";
    public GameLocation Location { get; set; } = GameLocation.Zero;
    
    public long JoinDate { get; set; } // unix milliseconds
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

    public string PlanetsHash { get; set; } = "0";

    public bool AllowIpAuthentication { get; set; }
    public string? CurrentVerifiedIp { get; set; }
    
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