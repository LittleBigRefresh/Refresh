using System.Xml.Serialization;
using Refresh.Common.Constants;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Data;
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
    
    [XmlElement("heartCount")] public required int HeartCount { get; set; }
    
    [XmlElement("playCount")] public required int TotalPlayCount { get; set; }
    [XmlElement("completionCount")] public required int CompletionCount { get; set; }
    [XmlElement("uniquePlayCount")] public required int UniquePlayCount { get; set; }

    [XmlElement("yourDPadRating")] public int YourRating { get; set; }
    [XmlElement("thumbsup")] public required int YayCount { get; set; }
    [XmlElement("thumbsdown")] public required int BooCount { get; set; }
    [XmlElement("yourRating")] public int YourStarRating { get; set; }
    
    // 1 by default since this will break reviews if set to 0 for GameLevelResponses that do not have extra data being filled in
    [XmlElement("yourlbp2PlayCount")] public int YourLbp2PlayCount { get; set; } = 1;
    
    [XmlArray("customRewards")]
    [XmlArrayItem("customReward")]
    public required List<GameSkillReward> SkillRewards { get; set; }

    [XmlElement("mmpick")] public required bool TeamPicked { get; set; }
    [XmlElement("resource")] public List<string> XmlResources { get; set; } = new();
    [XmlElement("playerCount")] public int PlayerCount { get; set; }
    
    [XmlElement("leveltype")] public required string LevelType { get; set; }
    
    [XmlElement("initiallyLocked")] public bool IsLocked { get; set; }
    [XmlElement("isSubLevel")] public bool IsSubLevel { get; set; }
    [XmlElement("shareable")] public int IsCopyable { get; set; }
    [XmlElement("backgroundGUID")] public string? BackgroundGuid { get; set; }
    [XmlElement("links")] public string? Links { get; set; }
    [XmlElement("averageRating")] public double AverageStarRating { get; set; }
    [XmlElement("sizeOfResources")] public int SizeOfResourcesInBytes { get; set; }
    [XmlElement("reviewCount")] public int ReviewCount { get; set; }
    [XmlElement("reviewsEnabled")] public bool ReviewsEnabled { get; set; } = true;
    [XmlElement("commentCount")] public int CommentCount { get; set; } = 0;
    [XmlElement("commentsEnabled")] public bool CommentsEnabled { get; set; } = true;
    [XmlElement("tags")] public string Tags { get; set; } = "";
    
    /// <summary>
    /// Provides a unique level ID for ~1.1 billion hashed levels, uses the hash directly, so this is deterministic
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static int LevelIdFromHash(string hash)
    {
        const int rangeStart = 1_000_000_000;
        const int rangeEnd = int.MaxValue;
        const int range = rangeEnd - rangeStart;
        
        return rangeStart + Math.Abs(hash.GetHashCode()) % range;
    }
    
    public static GameLevelResponse FromHash(string hash, DataContext dataContext)
    {
        return new GameLevelResponse
        {
            LevelId = dataContext.Game == TokenGame.LittleBigPlanet3 ? LevelIdFromHash(hash) : int.MaxValue,
            IsAdventure = false,
            Title = $"Hashed Level - {hash}",
            IconHash = "0",
            GameVersion = 0,
            RootResource = hash,
            Description = "This is a hashed level. We don't know anything about it.",
            Location = new GameLocation(),
            Handle = new SerializedUserHandle
            {
                Username = $"!Hashed",
                IconHash = "0",
            },
            Type = "user",
            TeamPicked = false,
            MinPlayers = 1,
            MaxPlayers = 4,
            HeartCount = 0,
            TotalPlayCount = 0,
            CompletionCount = 0,
            UniquePlayCount = 0,
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
            PublishDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            UpdateDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = [],
            LevelType = "",
        };
    }

    public static GameLevelResponse? FromOld(GameLevel? old, DataContext dataContext)
    {
        if (old == null) return null;

        GameLevelResponse response = new()
        {
            LevelId = old.LevelId,
            IsAdventure = old.IsAdventure,
            Title = old.Title,
            IconHash = old.IconHash,
            Description = old.Description,
            Location = new GameLocation(old.LocationX, old.LocationY),
            GameVersion = old.GameVersion.ToSerializedGame(),
            RootResource = old.RootResource,
            PublishDate = old.PublishDate.ToUnixTimeMilliseconds(),
            UpdateDate = old.UpdateDate.ToUnixTimeMilliseconds(),
            MinPlayers = old.MinPlayers,
            MaxPlayers = old.MaxPlayers,
            EnforceMinMaxPlayers = old.EnforceMinMaxPlayers,
            SameScreenGame = old.SameScreenGame,
            HeartCount = dataContext.Database.GetFavouriteCountForLevel(old),
            TotalPlayCount = dataContext.Database.GetTotalPlaysForLevel(old),
            CompletionCount = dataContext.Database.GetTotalCompletionsForLevel(old),
            UniquePlayCount = dataContext.Database.GetUniquePlaysForLevel(old),
            YayCount = dataContext.Database.GetTotalRatingsForLevel(old, RatingType.Yay),
            BooCount = dataContext.Database.GetTotalRatingsForLevel(old, RatingType.Boo),
            SkillRewards = old.SkillRewards.ToList(),
            TeamPicked = old.TeamPicked,
            LevelType = old.LevelType.ToGameString(),
            IsCopyable = old.IsCopyable ? 1 : 0,
            IsLocked = old.IsLocked,
            IsSubLevel = old.IsSubLevel,
            BackgroundGuid = old.BackgroundGuid,
            Links = "",
            AverageStarRating = old.CalculateAverageStarRating(dataContext.Database),
            ReviewCount = old.Reviews.Count,
            CommentCount = dataContext.Database.GetTotalCommentsForLevel(old),
            Tags = string.Join(',', dataContext.Database.GetTagsForLevel(old).Select(t => t.Tag.ToLbpString())) ,
        };

        response.Type = "user";
        if (old is { Publisher: not null, IsReUpload: false })
        {
            response.Handle = SerializedUserHandle.FromUser(old.Publisher, dataContext);
        }
        else
        {
            string publisher;
            if (!old.IsReUpload)
                publisher = SystemUsers.DeletedUserName;
            else
                publisher = string.IsNullOrEmpty(old.OriginalPublisher)
                    ? SystemUsers.UnknownUserName
                    : SystemUsers.SystemPrefix + old.OriginalPublisher;
            
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
            response.YourLbp2PlayCount = dataContext.Database.GetTotalPlaysForLevelByUser(old, dataContext.User);
        }
        
        response.PlayerCount = dataContext.Match.GetPlayerCountForLevel(RoomSlotType.Online, response.LevelId);
        
        GameAsset? rootResourceAsset = dataContext.Database.GetAssetFromHash(response.RootResource);
        if (rootResourceAsset != null)
        {
            rootResourceAsset.TraverseDependenciesRecursively(dataContext.Database, (_, asset) =>
            {
                if (asset != null)
                    response.SizeOfResourcesInBytes += asset.SizeInBytes;
            });
        }
        
        response.IconHash = dataContext.Database.GetAssetFromHash(old.IconHash)?.GetAsIcon(dataContext.Game, dataContext) ?? response.IconHash;
        
        response.CommentCount = dataContext.Database.GetTotalCommentsForLevel(old);
        
        return response;
    }

    public static IEnumerable<GameLevelResponse> FromOldList(IEnumerable<GameLevel> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}