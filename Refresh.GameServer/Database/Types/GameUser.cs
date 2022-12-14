using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.HttpServer.Authentication;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Database.Types;

[XmlRoot("user")]
public class GameUser : RealmObject, IUser, INeedsPreparationBeforeSerialization
{
    [PrimaryKey] [Indexed] [XmlIgnore] public ObjectId UserId { get; set; } = ObjectId.GenerateNewId();
    [Indexed] [Required] [XmlIgnore] public string Username { get; set; }

    [Ignored] [XmlElement("biography")] public string Description { get; set; } = "works";

    #region LBP Serialization Quirks
    [Ignored] [XmlAttribute("type")] public string Type { get; set; }
    [Ignored] [XmlElement("npHandle")] public NameAndIcon Handle { get; set; }
    
    [Ignored]
    public class NameAndIcon
    {
        [XmlText]
        public string Username { get; set; }
        
        [XmlAttribute("icon")]
        public string IconHash { get; set; }
    }

    public void PrepareForSerialization()
    {
        this.Type = "user";
        this.Handle = new NameAndIcon
        {
            Username = this.Username,
            IconHash = "",
        };
    }
    #endregion
}