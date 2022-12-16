using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer.Serialization;

namespace Refresh.GameServer.Types;

[XmlRoot("slot")]
public class GameLevel : RealmObject, INeedsPreparationBeforeSerialization
{
    [XmlIgnore] public ObjectId LevelId { get; set; } = ObjectId.GenerateNewId();
    
    [XmlElement("name")] public string Title { get; set; } = string.Empty;
    [XmlElement("icon")] public string IconHash { get; set; } = string.Empty;
    [XmlElement("description")] public string Description { get; set; } = string.Empty;
    [XmlElement("location")] public GameLocation Location { get; set; } = GameLocation.Zero;

    [XmlElement("resource")] public IList<string> Resources { get; } = new List<string>();

    public GameUser Publisher { get; set; }
    
    #region LBP Serialization Quirks
    [Ignored] [XmlAttribute("type")] public string? Type { get; set; }
    
    public void PrepareForSerialization()
    {
        this.Type = "user";
    }
    #endregion
}