using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer.Serialization;

namespace Refresh.GameServer.Types.Levels;

[XmlRoot("slot")]
[XmlType("slot")]
public class GameLevel : RealmObject, INeedsPreparationBeforeSerialization, ISequentialId
{
    [XmlElement("id")] public int LevelId { get; set; }
    
    [XmlElement("name")] public string Title { get; set; } = string.Empty;
    [XmlElement("icon")] public string IconHash { get; set; } = string.Empty;
    [XmlElement("description")] public string Description { get; set; } = string.Empty;
    [XmlElement("location")] public GameLocation Location { get; set; } = GameLocation.Zero;

    [XmlElement("rootLevel")] public string RootResource { get; set; } = string.Empty;
    [XmlIgnore] public IList<string> Resources { get; } = new List<string>();
    
    [XmlElement("firstPublished")] public long PublishDate { get; set; }
    [XmlElement("lastUpdated")] public long UpdateDate { get; set; }
    
    public int SequentialId
    {
        set => this.LevelId = value;
    }

    public GameUser? Publisher { get; set; }
    
    #region LBP Serialization Quirks
    [Ignored] [XmlAttribute("type")] public string? Type { get; set; }

    [Ignored] [XmlElement("npHandle")] public NameAndIcon? Handle { get; set; }

    // Realm cant have IList types with setters? Fine, I'll play your game. ;p
    [Ignored]
    [XmlElement("resource")]
    public string[] XmlResources
    {
        get => this.Resources.ToArray();
        set
        {
            this.Resources.Clear();
            
            foreach (string r in value) 
                this.Resources.Add(r);
        }
    }

    public void PrepareForSerialization()
    {
        this.Type = "user";
        this.Handle = new NameAndIcon
        {
            Username = this.Publisher.Username,
            IconHash = string.Empty,
        };
    }
    #endregion
}