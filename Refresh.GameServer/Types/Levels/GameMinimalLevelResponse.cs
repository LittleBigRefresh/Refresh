using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels;

public class GameMinimalLevelResponse : IDataConvertableFrom<GameMinimalLevelResponse, GameLevel>, IDataConvertableFrom<GameMinimalLevelResponse, GameLevelResponse>
{
    [XmlElement("name")] public required string Title { get; set; } = string.Empty;
    [XmlElement("icon")] public required string IconHash { get; set; } = string.Empty;
    [XmlElement("rootLevel")] public required string RootResource { get; set; } = string.Empty;
    [XmlElement("description")] public required string Description { get; set; } = string.Empty;
    [XmlElement("location")] public required GameLocation Location { get; set; } = GameLocation.Zero;
    [XmlElement("id")] public required int LevelId { get; set; }
    [XmlElement("npHandle")] public required SerializedUserHandle? Handle { get; set; }
    [XmlAttribute("type")] public required string? Type { get; set; }
    [XmlElement("mmpick")] public required bool TeamPicked { get; set; }
    [XmlElement("minPlayers")] public required int MinPlayers { get; set; }
    [XmlElement("maxPlayers")] public required int MaxPlayers { get; set; }
    [XmlElement("heartCount")] public required int HeartCount { get; set; }
    
    [XmlElement("playCount")] public required int TotalPlayCount { get; set; }
    [XmlElement("uniquePlayCount")] public required int UniquePlayCount { get; set; }
    [XmlElement("thumbsup")] public required int YayCount { get; set; }
    [XmlElement("thumbsdown")] public required int BooCount { get; set; }
    
    [XmlElement("playerCount")] public int PlayerCount { get; set; }
    
    private GameMinimalLevelResponse() {}
    
    public static GameMinimalLevelResponse? FromOldWithExtraData(GameLevelResponse? old, MatchService matchService)
    {
        if (old == null) return null;

        GameMinimalLevelResponse response = FromOld(old)!;
        response.FillInExtraData(matchService);

        return response;
    }
    
    public static GameMinimalLevelResponse? FromOldWithExtraData(GameLevel? old, MatchService matchService)
    {
        if (old == null) return null;

        GameMinimalLevelResponse response = FromOld(old)!;
        response.FillInExtraData(matchService);

        return response;
    }

    public static GameMinimalLevelResponse? FromOld(GameLevel? level)
    {
        if(level == null) return null;
        return FromOld(GameLevelResponse.FromOld(level));
    }


    public static GameMinimalLevelResponse? FromOld(GameLevelResponse? level)
    {
        if(level == null) return null;
        
        return new GameMinimalLevelResponse
        {
            Title = level.Title,
            IconHash = level.IconHash,
            RootResource = level.RootResource,
            Description = level.Description,
            Location = level.Location,
            LevelId = level.LevelId,
            Handle = level.Handle,
            Type = level.Type,
            TeamPicked = level.TeamPicked,
            YayCount = level.YayCount,
            BooCount = level.BooCount,
            HeartCount = level.HeartCount,
            MaxPlayers = level.MaxPlayers,
            MinPlayers = level.MinPlayers,
            TotalPlayCount = level.TotalPlayCount,
            UniquePlayCount = level.UniquePlayCount,
        };
    }

    private void FillInExtraData(MatchService matchService)
    {
        this.PlayerCount = matchService.GetPlayerCountForLevel(RoomSlotType.Online, this.LevelId);
    }

    public static IEnumerable<GameMinimalLevelResponse> FromOldList(IEnumerable<GameLevel> oldList) => oldList.Select(FromOld)!;
    public static IEnumerable<GameMinimalLevelResponse> FromOldList(IEnumerable<GameLevelResponse> oldList) => oldList.Select(FromOld)!;
}