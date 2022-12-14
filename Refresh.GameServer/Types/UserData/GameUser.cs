using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Comments;
using Refresh.HttpServer.Authentication;
using Refresh.HttpServer.Serialization;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("user")]
public partial class GameUser : RealmObject, IUser, INeedsPreparationBeforeSerialization
{
    [PrimaryKey] [Indexed] [XmlIgnore] public ObjectId UserId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] [Required] [XmlIgnore] public string Username { get; set; } = string.Empty;
    [XmlIgnore] public string IconHash { get; set; } = "0";

    [XmlElement("biography")] public string Description { get; set; } = "";
    [XmlElement("location")] public GameLocation Location { get; set; } = GameLocation.Zero;
    
    [XmlIgnore] public UserPins Pins { get; set; } = new();

#pragma warning disable CS8618
    [XmlIgnore] public IList<GameComment> ProfileComments { get; }
#pragma warning restore CS8618

    [XmlElement("planets")] public string PlanetsHash { get; set; } = "0";

    public override string ToString() => $"{this.Username} ({this.UserId})";

    #region LBP Serialization Quirks

    [Ignored] [XmlAttribute("type")] public string? Type { get; set; }
    [Ignored] [XmlElement("npHandle")] public NameAndIcon? Handle { get; set; }
    [Ignored] [XmlElement("commentCount")] public int? CommentCount { get; set; }
    [Ignored] [XmlElement("commentsEnabled")] public bool? CommentsEnabled { get; set; }

    private partial void SerializeSlots();

    public void PrepareForSerialization()
    {
        this.Type = "user";
        this.Handle = NameAndIcon.FromUser(this);
        this.CommentCount = this.ProfileComments.Count;
        this.CommentsEnabled = true;
        
        this.SerializeSlots();
    }
    #endregion
}