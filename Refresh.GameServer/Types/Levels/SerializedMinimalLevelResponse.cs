using System.Xml.Serialization;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels;

public class SerializedMinimalLevelResponse : IDataConvertableFrom<SerializedMinimalLevelResponse, GameLevel>, IDataConvertableFrom<SerializedMinimalLevelResponse, GamePlaylist>, IDataConvertableFrom<SerializedMinimalLevelResponse, GameLevelResponse>
{
    //NOTE: THIS MUST BE AT THE TOP OF THE XML RESPONSE OR ELSE LBP PSP WILL CRASH
    [XmlElement("id")] public required int LevelId { get; set; }
    
    [XmlElement("isAdventurePlanet")] public required bool IsAdventure { get; set; }
    [XmlElement("name")] public required string Title { get; set; } = string.Empty;
    [XmlElement("icon")] public required string IconHash { get; set; } = string.Empty;
    [XmlElement("game")] public required int GameVersion { get; set; }
    [XmlElement("rootLevel")] public required string RootResource { get; set; } = string.Empty;
    [XmlElement("description")] public required string Description { get; set; } = string.Empty;
    [XmlElement("location")] public required GameLocation Location { get; set; } = GameLocation.Zero;
    [XmlElement("npHandle")] public required SerializedUserHandle? Handle { get; set; }
    [XmlAttribute("type")] public required string? Type { get; set; }
    [XmlElement("leveltype")] public required string LevelType { get; set; }
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
    [XmlElement("moveRequired")] public bool RequiresMoveController { get; set; }
    [XmlElement("tags")] public string Tags { get; set; } = "";
 
    private SerializedMinimalLevelResponse() {}
    
    /// <summary>
    /// Constructs a placeholder level response from a root level hash
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="dataContext"></param>
    /// <returns></returns>
    public static SerializedMinimalLevelResponse FromHash(string hash, DataContext dataContext)
    {
        return FromOld(GameLevelResponse.FromHash(hash, dataContext), dataContext)!;
    }

    public static SerializedMinimalLevelResponse? FromOld(GameLevel? old, DataContext dataContext)
    {
        if(old == null) return null;
        return FromOld(GameLevelResponse.FromOld(old, dataContext), dataContext);
    }


    public static SerializedMinimalLevelResponse? FromOld(GameLevelResponse? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new SerializedMinimalLevelResponse
        {
            Title = old.Title,
            IsAdventure = old.IsAdventure,
            IconHash = dataContext.Database.GetAssetFromHash(old.IconHash)?.GetAsIcon(dataContext.Game, dataContext) ?? old.IconHash,
            GameVersion = old.GameVersion,
            RootResource = old.RootResource,
            Description = old.Description,
            Location = old.Location,
            LevelId = old.LevelId,
            Handle = old.Handle,
            Type = old.Type,
            LevelType = old.LevelType,
            TeamPicked = old.TeamPicked,
            YayCount = old.YayCount,
            BooCount = old.BooCount,
            HeartCount = old.HeartCount,
            MaxPlayers = old.MaxPlayers,
            MinPlayers = old.MinPlayers,
            TotalPlayCount = old.TotalPlayCount,
            UniquePlayCount = old.UniquePlayCount,
            YourStarRating = old.YourStarRating,
            YourRating = old.YourRating,
            AverageStarRating = old.AverageStarRating,
            CommentCount = old.CommentCount,
            IsLocked = old.IsLocked,
            IsSubLevel = old.IsSubLevel,
            IsCopyable = old.IsCopyable,
            RequiresMoveController = old.RequiresMoveController,
            PlayerCount = dataContext.Match.GetPlayerCountForLevel(RoomSlotType.Online, old.LevelId),
            Tags = old.Tags,
        };
    }
    
    public static SerializedMinimalLevelResponse? FromOld(GamePlaylist? old, DataContext dataContext)
    {
        if (old == null)
            return null;
        
        return new SerializedMinimalLevelResponse
        {
            LevelId = old.PlaylistId,
            IsAdventure = false,
            Title = old.Name,
            IconHash = dataContext.GetIconFromHash(old.IconHash),
            Description = old.Description,
            Type = GameSlotType.Playlist.ToGameType(),
            LevelType = GameSlotType.Playlist.ToGameType(),
            Location = new GameLocation(old.LocationX, old.LocationY),
            // Playlists are only ever serialized like this in LBP1-like builds, so we can assume LBP1
            GameVersion = TokenGame.LittleBigPlanet1.ToSerializedGame(),
            RootResource = "0",
            MinPlayers = 0,
            MaxPlayers = 0,
            HeartCount = 0, 
            TotalPlayCount = 0,
            UniquePlayCount = 0,
            YayCount = 0, 
            BooCount = 0,
            AverageStarRating = 0,
            YourStarRating = 0,
            YourRating = 0,
            PlayerCount = 0,
            ReviewsEnabled = true,
            ReviewCount = 0,
            CommentsEnabled = true,
            CommentCount = 0,
            IsLocked = false,
            IsSubLevel = false,
            IsCopyable = 0,
            RequiresMoveController = false,
            Tags = string.Empty, 
            TeamPicked = false, 
            Handle = SerializedUserHandle.FromUser(old.Publisher, dataContext),
        };
    }

    public static IEnumerable<SerializedMinimalLevelResponse> FromOldList(IEnumerable<GameLevel> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    public static IEnumerable<SerializedMinimalLevelResponse> FromOldList(IEnumerable<GameLevelResponse> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    public static IEnumerable<SerializedMinimalLevelResponse> FromOldList(IEnumerable<GamePlaylist> oldList, 
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;

}