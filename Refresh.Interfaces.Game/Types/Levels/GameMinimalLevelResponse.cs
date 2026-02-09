using System.Diagnostics;
using System.Xml.Serialization;
using Refresh.Common.Constants;
using Refresh.Core.Types.Data;
using Refresh.Core.Types.Matching;
using Refresh.Database.Models;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Interfaces.Game.Types.UserData;

namespace Refresh.Interfaces.Game.Types.Levels;

public class GameMinimalLevelResponse : IDataConvertableFrom<GameMinimalLevelResponse, GameLevel>, IDataConvertableFrom<GameMinimalLevelResponse, GamePlaylist>
{
    //NOTE: THIS MUST BE AT THE TOP OF THE XML RESPONSE OR ELSE LBP PSP WILL CRASH
    [XmlElement("id")] public required int LevelId { get; set; }
    
    [XmlElement("isAdventurePlanet")] public required bool IsAdventure { get; set; }
    [XmlElement("name")] public required string Title { get; set; } = string.Empty;
    [XmlElement("icon")] public string IconHash { get; set; } = string.Empty;
    [XmlElement("game")] public required int GameVersion { get; set; }
    [XmlElement("rootLevel")] public required string RootResource { get; set; } = string.Empty;
    [XmlElement("description")] public required string Description { get; set; } = string.Empty;
    [XmlElement("location")] public required GameLocation Location { get; set; } = GameLocation.Zero;
    [XmlElement("npHandle")] public SerializedUserHandle? Handle { get; set; } = null!;
    [XmlAttribute("type")] public required string? Type { get; set; }
    [XmlElement("leveltype")] public required string LevelType { get; set; }
    [XmlElement("mmpick")] public required bool TeamPicked { get; set; }
    [XmlElement("minPlayers")] public required int MinPlayers { get; set; }
    [XmlElement("maxPlayers")] public required int MaxPlayers { get; set; }
    [XmlElement("heartCount")] public required int HeartCount { get; set; }
    
    [XmlElement("playCount")] public required int TotalPlayCount { get; set; }
    [XmlElement("uniquePlayCount")] public required int UniquePlayCount { get; set; }
    [XmlElement("lbp1PlayCount")] public required int Lbp1TotalPlayCount { get; set; }
    [XmlElement("lbp1UniquePlayCount")] public required int Lbp1UniquePlayCount { get; set; }
    [XmlElement("lbp2PlayCount")] public required int Lbp2TotalPlayCount { get; set; }
    // There is no lbp2UniquePlayCount attribute in the packet captures
    [XmlElement("lbp3PlayCount")] public required int Lbp3TotalPlayCount { get; set; }
    [XmlElement("lbp3UniquePlayCount")] public required int Lbp3UniquePlayCount { get; set; }

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
    [XmlElement("authorLabels")] public string PublisherLabels { get; set; } = "";
    [XmlElement("labels")] public string AllLabels { get; set; } = ""; // Must also contain all publisher labels, else they won't show up

    protected GameMinimalLevelResponse() {}
    
    /// <summary>
    /// Constructs a placeholder level response from a root level hash
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="dataContext"></param>
    /// <returns></returns>
    public static GameMinimalLevelResponse FromHash(string hash, DataContext dataContext)
    {
        return new GameMinimalLevelResponse
        {
            LevelId = dataContext.Game == TokenGame.LittleBigPlanet3 ? GameLevel.LevelIdFromHash(hash) : int.MaxValue,
            IsAdventure = false,
            Title = $"Hashed Level - {hash}",
            IconHash = "0",
            GameVersion = 0,
            RootResource = hash,
            Description = "This is a hashed level from the Dry Archive. We can't provide any information about it.",
            Location = new GameLocation(),
            Handle = new SerializedUserHandle
            {
                Username = SystemUsers.HashedUserName,
                IconHash = "0",
            },
            Type = GameSlotType.User.ToGameType(),
            TeamPicked = false,
            MinPlayers = 1,
            MaxPlayers = 4,
            HeartCount = 0,
            TotalPlayCount = 0,
            UniquePlayCount = 0,
            Lbp1TotalPlayCount = 0,
            Lbp1UniquePlayCount = 0,
            Lbp2TotalPlayCount = 0,
            Lbp3TotalPlayCount = 0,
            Lbp3UniquePlayCount = 0,
            YayCount = 0,
            BooCount = 0,
            AverageStarRating = 0,
            YourStarRating = 0,
            YourRating = 0,
            PlayerCount = 0,
            ReviewsEnabled = false,
            ReviewCount = 0,
            CommentsEnabled = false,
            CommentCount = 0,
            IsLocked = false,
            IsSubLevel = false,
            IsCopyable = 0,
            RequiresMoveController = false,
            LevelType = "",
        };
    }

    public static GameMinimalLevelResponse? FromOld(GameLevel? old, DataContext dataContext)
    {
        if(old == null) return null;

        if(old.Statistics == null)
            dataContext.Database.RecalculateLevelStatistics(old);
        
        Debug.Assert(old.Statistics != null);

        bool isStoryLevel = old.StoryId != 0;

        GameMinimalLevelResponse response = new()
        {
            LevelId = isStoryLevel ? old.StoryId : old.LevelId,
            IsAdventure = old.IsAdventure,
            Title = old.Title,
            Description = old.Description,
            Location = new GameLocation(old.LocationX, old.LocationY),
            GameVersion = old.GameVersion.ToSerializedGame(),
            RootResource = old.RootResource,
            MinPlayers = old.MinPlayers,
            MaxPlayers = old.MaxPlayers,
            HeartCount = old.Statistics.FavouriteCount,
            TotalPlayCount = old.Statistics.PlayCount,
            UniquePlayCount = old.Statistics.UniquePlayCount,
            Lbp1TotalPlayCount = old.Statistics.PlayCount,
            Lbp1UniquePlayCount = old.Statistics.UniquePlayCount,
            Lbp2TotalPlayCount = old.Statistics.PlayCount,
            Lbp3TotalPlayCount = old.Statistics.PlayCount,
            Lbp3UniquePlayCount = old.Statistics.UniquePlayCount,
            YayCount = old.Statistics.YayCount,
            BooCount = old.Statistics.BooCount,
            TeamPicked = old.TeamPicked,
            LevelType = old.LevelType.ToGameString(),
            IsCopyable = old.IsCopyable ? 1 : 0,
            IsLocked = old.IsLocked,
            IsSubLevel = old.IsSubLevel,
            RequiresMoveController = old.RequiresMoveController,
            AverageStarRating = old.CalculateAverageStarRating(),
            ReviewCount = old.Statistics.ReviewCount,
            CommentCount = old.Statistics.CommentCount,
            Type = old.SlotType.ToGameType(),
        };
        
        // If we're not a reupload, show the real publisher
        // If we're the real publisher of a reupload, show the real publisher to give them editing capabilities
        if ((old.Publisher != null && !old.IsReUpload) || (dataContext.User != null && old.Publisher == dataContext.User && old.IsReUpload))
        {
            response.Handle = SerializedUserHandle.FromUser(old.Publisher, dataContext);
        }
        // Otherwise, show our special reupload username
        else
        {
            string publisher;
            if (!old.IsReUpload)
                publisher = SystemUsers.DeletedUserName;
            else
                publisher = string.IsNullOrEmpty(old.OriginalPublisher)
                    ? SystemUsers.UnknownUserName
                    : SystemUsers.SystemPrefix + old.OriginalPublisher;

            if (publisher.Length > 16) // Trim publisher name to fit in the maximum limit LBP will show
                publisher = string.Concat(publisher.AsSpan(0, 15), "-");
            
            
            response.Handle = new SerializedUserHandle
            {
                IconHash = "0",
                Username = publisher,
            };
        }
        
        if (dataContext.User != null)
        {
            RatingType? rating = dataContext.Database.GetRatingByUser(old, dataContext.User);

            response.YourRating = rating?.ToDPad() ?? (int)RatingType.Neutral;
            response.YourStarRating = rating?.ToLBP1() ?? 0;
        }
        
        response.PlayerCount = dataContext.Match.GetPlayerCountForLevel(RoomSlotType.Online, response.LevelId);

        if (dataContext.Game is TokenGame.LittleBigPlanet1 or TokenGame.BetaBuild)
        {
            response.Tags = string.Join(',', dataContext.Database.GetTagsForLevel(old).Select(t => t.ToLbpString()));
        }

        if (dataContext.Game is not TokenGame.LittleBigPlanet1 or TokenGame.LittleBigPlanetPSP)
        {
            // Try to deduplicate labels if the same ones appear among both the publisher and recurring labels
            response.AllLabels = old.Statistics.RecurringLabels.Concat(old.Labels).Distinct().ToLbpCommaList(dataContext.Game);
            response.PublisherLabels = old.Labels.ToLbpCommaList(dataContext.Game);
        }
        
        response.IconHash = dataContext.Database.GetAssetFromHash(old.IconHash)?.GetAsIcon(dataContext.Game, dataContext) ?? old.IconHash;
        return response;
    }
    
    public static GameMinimalLevelResponse? FromOld(GamePlaylist? old, DataContext dataContext)
    {
        if (old == null)
            return null;
        
        return new GameMinimalLevelResponse
        {
            LevelId = old.PlaylistId,
            IsAdventure = false,
            Title = old.Name,
            IconHash = dataContext.GetIconFromHash(old.IconHash),
            Description = old.Description,
            Type = GameSlotType.Playlist.ToGameType(),
            LevelType = GameLevelType.Normal.ToGameString(),
            Location = new GameLocation(old.LocationX, old.LocationY),
            // Playlists are only ever serialized like this in LBP1-like builds, so we can assume LBP1
            GameVersion = TokenGame.LittleBigPlanet1.ToSerializedGame(),
            RootResource = "0",
            MinPlayers = 0,
            MaxPlayers = 0,
            HeartCount = 0, 
            TotalPlayCount = 0,
            UniquePlayCount = 0,
            Lbp1TotalPlayCount = 0,
            Lbp1UniquePlayCount = 0,
            Lbp2TotalPlayCount = 0,
            Lbp3TotalPlayCount = 0,
            Lbp3UniquePlayCount = 0,
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

    public static IEnumerable<GameMinimalLevelResponse> FromOldList(IEnumerable<GameLevel> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;

    public static IEnumerable<GameMinimalLevelResponse> FromOldList(IEnumerable<GamePlaylist> oldList, 
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;

}