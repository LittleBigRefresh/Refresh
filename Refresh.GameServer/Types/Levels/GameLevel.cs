using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Bunkum.HttpServer.Serialization;
using Newtonsoft.Json;

namespace Refresh.GameServer.Types.Levels;

[XmlRoot("slot")]
[XmlType("slot")]
[JsonObject(MemberSerialization.OptIn)]
public class GameLevel : RealmObject, INeedsPreparationBeforeSerialization, ISequentialId
{
    [PrimaryKey] [Indexed] [XmlElement("id")] [JsonProperty] public int LevelId { get; set; }
    
    [XmlElement("name")] [JsonProperty] public string Title { get; set; } = string.Empty;
    [XmlElement("icon")] [JsonProperty] public string IconHash { get; set; } = string.Empty;
    [XmlElement("description")] [JsonProperty] public string Description { get; set; } = string.Empty;
    [XmlElement("location")] [JsonProperty] public GameLocation Location { get; set; } = GameLocation.Zero;

    [XmlElement("rootLevel")] public string RootResource { get; set; } = string.Empty;
    [XmlIgnore] public IList<string> Resources { get; } = new List<string>();
    
    [XmlElement("firstPublished")] [JsonProperty] public long PublishDate { get; set; } // unix seconds
    [XmlElement("lastUpdated")] [JsonProperty] public long UpdateDate { get; set; }
    
    public int SequentialId
    {
        set => this.LevelId = value;
    }

    [JsonProperty] public GameUser? Publisher { get; set; }
    
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
        if (this.Publisher != null)
        {
            this.Type = "user";
            this.Handle = NameAndIcon.FromUser(this.Publisher);
        }
        else
        {
            this.Type = "developer";
        }
    }
    #endregion
}