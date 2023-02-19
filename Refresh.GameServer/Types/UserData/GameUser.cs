using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Comments;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Serialization;
using Newtonsoft.Json;
using Refresh.GameServer.Types.Relations;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("user")]
[JsonObject(MemberSerialization.OptIn)]
public partial class GameUser : RealmObject, IUser, INeedsPreparationBeforeSerialization
{
    [PrimaryKey] [Indexed] [XmlIgnore] [JsonProperty] public ObjectId UserId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] [Required] [XmlIgnore] [JsonProperty] public string Username { get; set; } = string.Empty;
    [XmlIgnore] [JsonProperty] public string IconHash { get; set; } = "0";

    [XmlElement("biography")] [JsonProperty] public string Description { get; set; } = "";
    [XmlElement("location")] [JsonProperty] public GameLocation Location { get; set; } = GameLocation.Zero;
    
    [XmlIgnore] public UserPins Pins { get; set; } = new();

#pragma warning disable CS8618
    [XmlIgnore] public IList<GameComment> ProfileComments { get; }
#pragma warning restore CS8618
    
    #nullable disable
    [Backlink(nameof(FavouriteLevelRelation.User))]
    [XmlIgnore] public IQueryable<FavouriteLevelRelation> FavouriteLevelRelations { get; }
    
    [Backlink(nameof(FavouriteLevelRelation.User))]
    [XmlIgnore] public IQueryable<QueueLevelRelation> QueueLevelRelations { get; }
    #nullable restore

    [XmlElement("planets")] public string PlanetsHash { get; set; } = "0";

    public override string ToString() => $"{this.Username} ({this.UserId})";

    #region LBP Serialization Quirks

    [Ignored] [XmlAttribute("type")] public string? Type { get; set; }
    [Ignored] [XmlElement("npHandle")] public NameAndIcon? Handle { get; set; }
    [Ignored] [XmlElement("commentCount")] public int? CommentCount { get; set; }
    [Ignored] [XmlElement("commentsEnabled")] public bool? CommentsEnabled { get; set; }
    [Ignored] [XmlElement("favouriteSlotCount")] public int? FavouriteLevelCount { get; set; }
    [Ignored] [XmlElement("lolcatftwCount")] public int? QueuedLevelCount { get; set; }

    private partial void SerializeSlots();

    public void PrepareForSerialization()
    {
        this.Type = "user";
        this.Handle = NameAndIcon.FromUser(this);
        
        this.CommentCount = this.ProfileComments.Count;
        this.CommentsEnabled = true;

        this.FavouriteLevelCount = this.FavouriteLevelRelations.Count();
        this.QueuedLevelCount = this.QueueLevelRelations.Count();
        
        this.SerializeSlots();
    }
    #endregion
}