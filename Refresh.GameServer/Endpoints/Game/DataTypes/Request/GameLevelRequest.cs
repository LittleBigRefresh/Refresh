using System.Xml.Serialization;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.SkillRewards;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.DataTypes.Request;

[XmlRoot("slot")]
[XmlType("slot")]
public class GameLevelRequest
{
    [XmlElement("id")] public required int LevelId { get; set; }

    [XmlElement("name")] public required string Title { get; set; }
    [XmlElement("icon")] public required string IconHash { get; set; }
    [XmlElement("description")] public required string Description { get; set; }
    [XmlElement("location")] public required GameLocation Location { get; set; }

    [XmlElement("game")] public required int GameVersion { get; set; }
    [XmlElement("rootLevel")] public required string RootResource { get; set; }

    [XmlElement("firstPublished")] public required long PublishDate { get; set; } // unix seconds
    [XmlElement("lastUpdated")] public required long UpdateDate { get; set; }
    
    [XmlElement("minPlayers")] public required int MinPlayers { get; set; }
    [XmlElement("maxPlayers")] public required int MaxPlayers { get; set; }
    [XmlElement("enforceMinMaxPlayers")] public required bool EnforceMinMaxPlayers { get; set; }
    
    [XmlElement("sameScreenGame")] public required bool SameScreenGame { get; set; }

    [XmlAttribute("type")] public string Type { get; set; } = "user";

    [XmlElement("npHandle")] public SerializedUserHandle Handle { get; set; } = null!;
    
    [XmlArray("customRewards")]
    [XmlArrayItem("customReward")]
    public required List<GameSkillReward> SkillRewards { get; set; }

    [XmlElement("resource")] public List<string> XmlResources { get; set; } = new();
    [XmlElement("leveltype")] public string? LevelType { get; set; } = "";

    [XmlElement("initiallyLocked")] public bool IsLocked { get; set; }
    [XmlElement("isSubLevel")] public bool IsSubLevel { get; set; }
    [XmlElement("shareable")] public int IsCopyable { get; set; }
    
    [XmlElement("backgroundGUID")] public string? BackgroundGuid { get; set; }
    
    public GameLevel ToGameLevel(GameUser publisher) =>
        new()
        {
            LevelId = this.LevelId,
            Title = this.Title,
            IconHash = this.IconHash,
            Description = this.Description,
            Location = this.Location,
            RootResource = this.RootResource,
            PublishDate = this.PublishDate,
            UpdateDate = this.UpdateDate,
            MinPlayers = this.MinPlayers,
            MaxPlayers = this.MaxPlayers,
            EnforceMinMaxPlayers = this.EnforceMinMaxPlayers,
            SameScreenGame = this.SameScreenGame,
            SkillRewards = this.SkillRewards.ToArray(),
            Publisher = publisher,
            LevelType = GameLevelTypeExtensions.FromGameString(this.LevelType),
            IsLocked = this.IsLocked,
            IsSubLevel = this.IsSubLevel,
            IsCopyable = this.IsCopyable == 1,
            BackgroundGuid = this.BackgroundGuid,
        };
}