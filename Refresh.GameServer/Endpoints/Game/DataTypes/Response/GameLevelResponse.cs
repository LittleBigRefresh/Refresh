using System.Xml.Serialization;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.SkillRewards;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.DataTypes.Response;

[XmlRoot("slot")]
[XmlType("slot")]
public class GameLevelResponse : IDataConvertableFrom<GameLevelResponse, GameLevel>
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
    
    [XmlElement("heartCount")] public required int HeartCount { get; set; }
    
    [XmlElement("playCount")] public required int TotalPlayCount { get; set; }
    [XmlElement("uniquePlayCount")] public required int UniquePlayCount { get; set; }

    [XmlElement("yourDPadRating")] public int YourRating { get; set; }
    [XmlElement("thumbsup")] public required int YayCount { get; set; }
    [XmlElement("thumbsdown")] public required int BooCount { get; set; }
    
    [XmlArray("customRewards")]
    [XmlArrayItem("customReward")]
    public required List<GameSkillReward> SkillRewards { get; set; }

    [XmlElement("mmpick")] public required bool TeamPicked { get; set; }
    [XmlElement("resource")] public List<string> XmlResources { get; set; } = new();
    [XmlElement("playerCount")] public int PlayerCount { get; set; }

    public static GameLevelResponse? FromOldWithExtraData(GameLevel? old, GameDatabaseContext database, MatchService matchService, GameUser user)
    {
        if (old == null) return null;

        GameLevelResponse response = FromOld(old)!;
        response.FillInExtraData(database, matchService, user);

        return response;
    }

    public static GameLevelResponse? FromOld(GameLevel? old)
    {
        if (old == null) return null;

        GameLevelResponse response = new()
        {
            LevelId = old.LevelId,
            Title = old.Title,
            IconHash = old.IconHash,
            Description = old.Description,
            Location = old.Location,
            GameVersion = old.GameVersion.ToSerializedGame(),
            RootResource = old.RootResource,
            PublishDate = old.PublishDate,
            UpdateDate = old.UpdateDate,
            MinPlayers = old.MinPlayers,
            MaxPlayers = old.MaxPlayers,
            EnforceMinMaxPlayers = old.EnforceMinMaxPlayers,
            SameScreenGame = old.SameScreenGame,
            HeartCount = old.FavouriteRelations.Count(),
            TotalPlayCount = old.AllPlays.Count(),
            UniquePlayCount = old.UniquePlays.Count(),
            YayCount = old.Ratings.Count(r => r._RatingType == (int)RatingType.Yay),
            BooCount = old.Ratings.Count(r => r._RatingType == (int)RatingType.Boo),
            SkillRewards = old.SkillRewards.ToList(),
            TeamPicked = old.TeamPicked,
        };

        if (old.Publisher == null)
        {
            response.Type = "developer";
        }
        else
        {
            response.Type = "user";
            response.Handle = SerializedUserHandle.FromUser(old.Publisher);
        }

        return response;
    }

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
        };

    public static IEnumerable<GameLevelResponse> FromOldList(IEnumerable<GameLevel> oldList) => oldList.Select(FromOld)!;

    private void FillInExtraData(GameDatabaseContext database, MatchService matchService, GameUser user)
    {
        GameLevel? level = database.GetLevelById(this.LevelId);
        if (level == null) throw new InvalidOperationException("Cannot fill in level data for a level that does not exist.");
        
        this.YourRating = (int)database.GetRatingByUser(level, user);
        this.PlayerCount = matchService.GetPlayerCountForLevel(RoomSlotType.Online, this.LevelId);
    }
}