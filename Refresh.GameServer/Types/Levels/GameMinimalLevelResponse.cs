using System.Xml.Serialization;
using Bunkum.Core.Storage;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels;

public class GameMinimalLevelResponse : IDataConvertableFrom<GameMinimalLevelResponse, GameLevel>, IDataConvertableFrom<GameMinimalLevelResponse, GameLevelResponse>
{
    //NOTE: THIS MUST BE AT THE TOP OF THE XML RESPONSE OR ELSE LBP PSP WILL CRASH
    [XmlElement("id")] public required int LevelId { get; set; }
    
    [XmlElement("name")] public required string Title { get; set; } = string.Empty;
    [XmlElement("icon")] public required string IconHash { get; set; } = string.Empty;
    [XmlElement("game")] public required int GameVersion { get; set; }
    [XmlElement("rootLevel")] public required string RootResource { get; set; } = string.Empty;
    [XmlElement("description")] public required string Description { get; set; } = string.Empty;
    [XmlElement("location")] public required GameLocation Location { get; set; } = GameLocation.Zero;
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
    [XmlElement("averageRating")] public double AverageStarRating { get; set; }
    [XmlElement("yourRating")] public int YourStarRating { get; set; }
    [XmlElement("yourDPadRating")] public int YourRating { get; set; }

    [XmlElement("playerCount")] public int PlayerCount { get; set; }
    [XmlElement("reviewsEnabled")] public bool ReviewsEnabled { get; set; } = true;
    [XmlElement("reviewCount")] public int ReviewCount { get; set; } = 0;
    [XmlElement("commentsEnabled")] public bool CommentsEnabled { get; set; } = true;
    [XmlElement("commentCount")] public int CommentCount { get; set; } = 0; 
    
    [XmlElement("initiallyLocked")] public bool IsLocked { get; set; }
    [XmlElement("isSubLevel")] public bool IsSubLevel { get; set; }
    [XmlElement("shareable")] public int IsCopyable { get; set; }
    
    private GameMinimalLevelResponse() {}
    
    /// <summary>
    /// Constructs a placeholder level response from a root level hash
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="dataContext"></param>
    /// <returns></returns>
    public static GameMinimalLevelResponse FromHash(string hash, DataContext dataContext)
    {
        return FromOld(GameLevelResponse.FromHash(hash), dataContext)!;
    }
    
    public static GameMinimalLevelResponse? FromOldWithExtraData(GameLevelResponse? old, MatchService matchService,
        GameDatabaseContext database, IDataStore dataStore, TokenGame game, DataContext dataContext)
    {
        if (old == null) return null;

        GameMinimalLevelResponse response = FromOld(old, dataContext)!;
        response.FillInExtraData(matchService, database, dataStore, game);

        return response;
    }
    
    public static GameMinimalLevelResponse? FromOldWithExtraData(GameLevel? old, MatchService matchService,
        GameDatabaseContext database, IDataStore dataStore, TokenGame game, DataContext dataContext)
    {
        if (old == null) return null;

        GameMinimalLevelResponse response = FromOld(old, dataContext)!;
        response.FillInExtraData(matchService, database, dataStore, game);

        return response;
    }

    public static GameMinimalLevelResponse? FromOld(GameLevel? level, DataContext dataContext)
    {
        if(level == null) return null;
        return FromOld(GameLevelResponse.FromOld(level, dataContext), dataContext);
    }


    public static GameMinimalLevelResponse? FromOld(GameLevelResponse? level, DataContext dataContext)
    {
        if(level == null) return null;

        return new GameMinimalLevelResponse
        {
            Title = level.Title,
            IconHash = level.IconHash,
            GameVersion = level.GameVersion,
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
            YourStarRating = level.YourStarRating,
            YourRating = level.YourRating,
            AverageStarRating = level.AverageStarRating,
            CommentCount = level.CommentCount,
            IsLocked = level.IsLocked,
            IsSubLevel = level.IsSubLevel,
            IsCopyable = level.IsCopyable,
        };
    }

    private void FillInExtraData(MatchService matchService, GameDatabaseContext database, IDataStore dataStore, TokenGame game)
    {
        this.PlayerCount = matchService.GetPlayerCountForLevel(RoomSlotType.Online, this.LevelId);
        this.IconHash = database.GetAssetFromHash(this.IconHash)?.GetAsIcon(game, database, dataStore) ?? this.IconHash;
    }

    public static IEnumerable<GameMinimalLevelResponse> FromOldList(IEnumerable<GameLevel> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    public static IEnumerable<GameMinimalLevelResponse> FromOldList(IEnumerable<GameLevelResponse> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}