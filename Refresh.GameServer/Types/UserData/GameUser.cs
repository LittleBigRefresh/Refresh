using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Comments;
using Refresh.HttpServer.Authentication;
using Refresh.HttpServer.Serialization;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("user")]
public class GameUser : RealmObject, IUser, INeedsPreparationBeforeSerialization
{
    [PrimaryKey] [Indexed] [XmlIgnore] public ObjectId UserId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] [Required] [XmlIgnore] public string Username { get; set; } = string.Empty;
    [XmlIgnore] public string IconHash { get; set; } = "0";

    [XmlElement("biography")] public string Description { get; set; } = "";
    
    [XmlElement("location")] public GameLocation Location { get; set; } = GameLocation.Zero;
    
    [XmlIgnore] public UserPins Pins { get; set; } = new();

    [XmlIgnore] public IList<GameComment> ProfileComments { get; }

    #region LBP Serialization Quirks

    [Ignored] [XmlAttribute("type")] public string? Type { get; set; }
    [Ignored] [XmlElement("npHandle")] public NameAndIcon? Handle { get; set; }
    [Ignored] [XmlElement("commentCount")] public int? CommentCount { get; set; }
    [Ignored] [XmlElement("commentsEnabled")] public bool? CommentsEnabled { get; set; }
    
    // TODO: Move the hellscape below to a source generator/partial class
    [Ignored] [XmlElement("freeSlots")] public int? FreeSlots { get; set; }
    [Ignored] [XmlElement("lbp2FreeSlots")] public int? FreeSlotsLBP2 { get; set; }
    [Ignored] [XmlElement("lbp3FreeSlots")] public int? FreeSlotsLBP3 { get; set; }
    [Ignored] [XmlElement("entitledSlots")] public int? EntitledSlots { get; set; }
    [Ignored] [XmlElement("lbp2EntitledSlots")] public int? EntitledSlotsLBP2 { get; set; }
    [Ignored] [XmlElement("lbp3EntitledSlots")] public int? EntitledSlotsLBP3 { get; set; }
    [Ignored] [XmlElement("lbp1UsedSlots")] public int? UsedSlots { get; set; }
    [Ignored] [XmlElement("lbp2UsedSlots")] public int? UsedSlotsLBP2 { get; set; }
    [Ignored] [XmlElement("lbp3UsedSlots")] public int? UsedSlotsLBP3 { get; set; }
    [Ignored] [XmlElement("lbp2PurchasedSlots")] public int? PurchasedSlotsLBP2 { get; set; }
    [Ignored] [XmlElement("lbp3PurchasedSlots")] public int? PurchasedSlotsLBP3 { get; set; }

    public void PrepareForSerialization()
    {
        this.Type = "user";
        this.Handle = NameAndIcon.FromUser(this);
        this.CommentCount = this.ProfileComments.Count;
        this.CommentsEnabled = true;
        
        this.FreeSlots = 20;
        this.FreeSlotsLBP2 = 20;
        this.FreeSlotsLBP3 = 20;
        
        this.EntitledSlots = 20;
        this.EntitledSlotsLBP2 = 20;
        this.EntitledSlotsLBP3 = 20;
        
        this.UsedSlots = 1;
        this.UsedSlotsLBP2 = 1;
        this.UsedSlotsLBP3 = 1;
        
        this.PurchasedSlotsLBP2 = 0;
        this.PurchasedSlotsLBP3 = 0;
    }
    #endregion
}