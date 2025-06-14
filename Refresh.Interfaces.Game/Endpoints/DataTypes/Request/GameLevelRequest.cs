using System.Xml.Serialization;
using Refresh.Database.Models;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.UserData;

namespace Refresh.Interfaces.Game.Endpoints.DataTypes.Request;

[XmlRoot("slot")]
[XmlType("slot")]
public class GameLevelRequest
{
    [XmlElement("id")] public required int LevelId { get; set; }
    
    [XmlElement("isAdventurePlanet")] public required bool IsAdventure { get; set; }

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
    [XmlElement("moveRequired")] public bool RequiresMoveController { get; set; }
    
    [XmlElement("backgroundGUID")] public string? BackgroundGuid { get; set; }
    
    [XmlArray("slots")] public GameLevelRequest[]? Slots { get; set; }
    
    public GameLevel ToGameLevel(GameUser publisher) =>
        new()
        {
            LevelId = this.LevelId,
            IsAdventure = this.IsAdventure,
            Title = this.Title,
            IconHash = this.IconHash,
            Description = this.Description,
            LocationX = this.Location.X,
            LocationY = this.Location.Y,
            RootResource = this.RootResource,
            PublishDate = DateTimeOffset.FromUnixTimeMilliseconds(this.PublishDate),
            UpdateDate = DateTimeOffset.FromUnixTimeMilliseconds(this.UpdateDate),
            MinPlayers = this.MinPlayers,
            MaxPlayers = this.MaxPlayers,
            EnforceMinMaxPlayers = this.EnforceMinMaxPlayers,
            SameScreenGame = this.SameScreenGame,
            Publisher = publisher,
            LevelType = GameLevelTypeExtensions.FromGameString(this.LevelType),
            IsLocked = this.IsLocked,
            IsSubLevel = this.IsSubLevel,
            IsCopyable = this.IsCopyable == 1,
            RequiresMoveController = this.RequiresMoveController,
            BackgroundGuid = this.BackgroundGuid,
        };
}