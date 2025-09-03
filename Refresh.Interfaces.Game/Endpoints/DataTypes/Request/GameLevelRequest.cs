using System.Xml.Serialization;
using Refresh.Database.Models;
using Refresh.Database.Models.Levels;
using Refresh.Database.Query;

namespace Refresh.Interfaces.Game.Endpoints.DataTypes.Request;

[XmlRoot("slot")]
[XmlType("slot")]
public class GameLevelRequest : ISerializedPublishLevel
{
    // Not included if its not a republish, in that case default to 0 (invalid ID)
    [XmlElement("id")] public int LevelId { get; set; }

    [XmlElement("name")] public string Title { get; set; } = "";
    [XmlElement("icon")] public string IconHash { get; set; } = "0";
    [XmlElement("description")] public string Description { get; set; } = "";
    [XmlElement("location")] public GameLocation Location { get; set; } = GameLocation.Zero;

    [XmlElement("authorLabels")] public string PublisherLabels { get; set; } = "";
    [XmlIgnore] public IEnumerable<Label> FinalPublisherLabels { get; set; } = [];

    [XmlElement("rootLevel")] public required string RootResource { get; set; }
    [XmlElement("isAdventurePlanet")] public bool IsAdventure { get; set; }

    [XmlElement("minPlayers")] public int MinPlayers { get; set; } = 1;
    [XmlElement("maxPlayers")] public int MaxPlayers { get; set; } = 4;
    [XmlElement("enforceMinMaxPlayers")] public bool EnforceMinMaxPlayers { get; set; }
    
    [XmlElement("sameScreenGame")] public bool SameScreenGame { get; set; }

    [XmlArray("customRewards")]
    [XmlArrayItem("customReward")]
    public List<GameSkillReward> SkillRewards { get; set; } = [];

    [XmlElement("resource")] public List<string> XmlResources { get; set; } = new();
    [XmlElement("leveltype")] public string LevelType { get; set; } = "";

    [XmlElement("initiallyLocked")] public bool IsLocked { get; set; }
    [XmlElement("isSubLevel")] public bool IsSubLevel { get; set; }
    [XmlElement("shareable")] public int IsCopyable { get; set; }
    [XmlElement("moveRequired")] public bool RequiresMoveController { get; set; }
    
    [XmlElement("backgroundGUID")] public string? BackgroundGuid { get; set; }
    
    [XmlArray("slots")] public GameLevelRequest[]? Slots { get; set; }
}