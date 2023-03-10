using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Comments;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Serialization;
using Newtonsoft.Json;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("user")]
[JsonObject(MemberSerialization.OptIn)]
public partial class GameUser : RealmObject, IUser, INeedsPreparationBeforeSerialization
{
    [PrimaryKey] [Indexed] [XmlIgnore] [JsonProperty] public ObjectId UserId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] [Required] [XmlIgnore] [JsonProperty] public string Username { get; set; } = string.Empty;
    [Indexed] [XmlIgnore] [JsonIgnore] public string? PasswordBcrypt { get; set; } = null;
    [XmlIgnore] [JsonProperty] public string IconHash { get; set; } = "0";

    [XmlElement("biography")] [JsonProperty] public string Description { get; set; } = "";
    [XmlElement("location")] [JsonProperty] public GameLocation Location { get; set; } = GameLocation.Zero;
    
    [XmlIgnore] public UserPins Pins { get; set; } = new();
    
    #nullable disable
    [XmlIgnore] public IList<GameComment> ProfileComments { get; }
    
    [Backlink(nameof(FavouriteLevelRelation.User))]
    [XmlIgnore] public IQueryable<FavouriteLevelRelation> FavouriteLevelRelations { get; }
    
    [Backlink(nameof(QueueLevelRelation.User))]
    [XmlIgnore] public IQueryable<QueueLevelRelation> QueueLevelRelations { get; }
    
    [Backlink(nameof(FavouriteUserRelation.UserToFavourite))]
    [XmlIgnore] public IQueryable<FavouriteUserRelation> UsersFavouritingMe { get; }
    
    [Backlink(nameof(FavouriteUserRelation.UserFavouriting))]
    [XmlIgnore] public IQueryable<FavouriteUserRelation> UsersFavourited { get; }

    [Backlink(nameof(GameLevel.Publisher))]
    [XmlIgnore] public IQueryable<GameLevel> PublishedLevels { get; }
    #nullable restore

    [XmlElement("planets")] public string PlanetsHash { get; set; } = "0";

    public override string ToString() => $"{this.Username} ({this.UserId})";

    #region LBP Serialization Quirks

    [Ignored] [XmlAttribute("type")] public string? Type { get; set; }
    [Ignored] [XmlElement("npHandle")] public NameAndIcon? Handle { get; set; }
    [Ignored] [XmlElement("commentCount")] public int? CommentCount { get; set; }
    [Ignored] [XmlElement("commentsEnabled")] public bool? CommentsEnabled { get; set; }
    [Ignored] [XmlElement("favouriteSlotCount")] public int? FavouriteLevelCount { get; set; }
    [Ignored] [XmlElement("favouriteUserCount")] public int? FavouriteUserCount { get; set; }
    [Ignored] [XmlElement("lolcatftwCount")] public int? QueuedLevelCount { get; set; }
    [Ignored] [XmlElement("heartCount")] public int? HeartCount { get; set; }

    private partial void SerializeSlots();

    public void PrepareForSerialization()
    {
        this.Type = "user";
        this.Handle = NameAndIcon.FromUser(this);
        
        this.CommentCount = this.ProfileComments.Count;
        this.CommentsEnabled = true;

        this.FavouriteLevelCount = this.FavouriteLevelRelations.Count();
        this.FavouriteUserCount = this.UsersFavourited.Count();
        this.QueuedLevelCount = this.QueueLevelRelations.Count();
        this.HeartCount = this.UsersFavouritingMe.Count();
        
        this.SerializeSlots();
    }
    #endregion
}