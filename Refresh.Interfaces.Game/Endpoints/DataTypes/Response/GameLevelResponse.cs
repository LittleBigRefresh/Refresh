using System.Diagnostics;
using System.Xml.Serialization;
using Refresh.Common.Constants;
using Refresh.Core.Types.Data;
using Refresh.Database.Models;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Reviews;
using Refresh.Interfaces.Game.Types.UserData;

namespace Refresh.Interfaces.Game.Endpoints.DataTypes.Response;

[XmlRoot("slot")]
[XmlType("slot")]
public class GameLevelResponse : IDataConvertableFrom<GameLevelResponse, GameLevel>, IDataConvertableFrom<GameLevelResponse, GamePlaylist>
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

    [XmlAttribute("type")] public string? Type { get; set; } = GameSlotType.User.ToGameType();

    [XmlElement("npHandle")] public SerializedUserHandle Handle { get; set; } = null!;
    
    [XmlElement("heartCount")] public required int HeartCount { get; set; }
    
    [XmlElement("playCount")] public required int TotalPlayCount { get; set; }
    [XmlElement("completionCount")] public required int CompletionCount { get; set; }
    [XmlElement("uniquePlayCount")] public required int UniquePlayCount { get; set; }
    [XmlElement("lbp3PlayCount")] public required int Lbp3PlayCount { get; set; }

    [XmlElement("yourDPadRating")] public int YourRating { get; set; }
    [XmlElement("thumbsup")] public required int YayCount { get; set; }
    [XmlElement("thumbsdown")] public required int BooCount { get; set; }
    [XmlElement("yourRating")] public int YourStarRating { get; set; }

    [XmlElement("yourlbp1PlayCount")] public int YourLbp1PlayCount { get; set; }
    [XmlElement("yourlbp2PlayCount")] public int YourLbp2PlayCount { get; set; }
    [XmlElement("yourlbp3PlayCount")] public int YourLbp3PlayCount { get; set; }
    [XmlArray("customRewards")]
    [XmlArrayItem("customReward")]
    public List<GameSkillReward> SkillRewards { get; set; } = [];

    [XmlElement("mmpick")] public required bool TeamPicked { get; set; }
    [XmlElement("resource")] public List<string> XmlResources { get; set; } = new();
    [XmlElement("playerCount")] public int PlayerCount { get; set; }
    
    [XmlElement("leveltype")] public required string LevelType { get; set; }
    
    [XmlElement("initiallyLocked")] public bool IsLocked { get; set; }
    [XmlElement("isSubLevel")] public bool IsSubLevel { get; set; }
    [XmlElement("shareable")] public int IsCopyable { get; set; }
    [XmlElement("moveRequired")] public bool RequiresMoveController { get; set; }
    [XmlElement("backgroundGUID")] public string? BackgroundGuid { get; set; }
    [XmlElement("links")] public string? Links { get; set; }
    [XmlElement("averageRating")] public double AverageStarRating { get; set; }
    [XmlElement("sizeOfResources")] public int SizeOfResourcesInBytes { get; set; }
    [XmlElement("reviewCount")] public int ReviewCount { get; set; }
    [XmlElement("reviewsEnabled")] public bool ReviewsEnabled { get; set; } = true;
    [XmlElement("yourReview")] public SerializedGameReview? YourReview { get; set; }
    [XmlElement("commentCount")] public int CommentCount { get; set; } = 0;
    [XmlElement("commentsEnabled")] public bool CommentsEnabled { get; set; } = true;
    [XmlElement("photoCount")] public int PhotoCount { get; set; }
    [XmlElement("authorPhotoCount")] public int PublisherPhotoCount { get; set; }
    [XmlElement("tags")] public string Tags { get; set; } = "";
    
    public static GameLevelResponse FromHash(string hash, DataContext dataContext)
    {
        GameMinimalLevelResponse minimal = GameMinimalLevelResponse.FromHash(hash, dataContext);

        return new GameLevelResponse
        {
            LevelId = minimal.LevelId,
            IsAdventure = false,
            Title = minimal.Title,
            IconHash = "0",
            GameVersion = 0,
            RootResource = hash,
            Description = minimal.Description,
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
            CompletionCount = 0,
            UniquePlayCount = 0,
            Lbp3PlayCount = 0,
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
            PhotoCount = 0,
            PublisherPhotoCount = 0,
            IsLocked = false,
            IsSubLevel = false,
            IsCopyable = 0,
            RequiresMoveController = false,
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
        GameMinimalLevelResponse? minimal = GameMinimalLevelResponse.FromOld(old, dataContext);
        if (old == null || minimal == null) return null;

        Debug.Assert(old.Statistics != null);

        GameLevelResponse response = new()
        {
            // Provided by GameMinimalLevelResponse
            LevelId = minimal.LevelId,
            IsAdventure = minimal.IsAdventure,
            Title = minimal.Title,
            IconHash = minimal.IconHash,
            Description = minimal.Description,
            Location = minimal.Location,
            GameVersion = minimal.GameVersion,
            RootResource = minimal.RootResource,
            Handle = minimal.Handle!,
            Type = minimal.Type,
            LevelType = minimal.LevelType,
            HeartCount = minimal.HeartCount,
            TotalPlayCount = minimal.TotalPlayCount,
            UniquePlayCount = minimal.UniquePlayCount,
            Lbp3PlayCount = minimal.Lbp3PlayCount,
            YayCount = minimal.YayCount,
            BooCount = minimal.BooCount,
            AverageStarRating = minimal.AverageStarRating,
            YourRating = minimal.YourRating,
            YourStarRating = minimal.YourStarRating,
            ReviewCount = minimal.ReviewCount,
            CommentCount = minimal.CommentCount,
            PlayerCount = minimal.PlayerCount,
            TeamPicked = minimal.TeamPicked,
            IsCopyable = minimal.IsCopyable,
            IsLocked = minimal.IsLocked,
            IsSubLevel = minimal.IsSubLevel,
            RequiresMoveController = minimal.RequiresMoveController,
            Tags = minimal.Tags,

            // Not provided by GameMinimalLevelResponse
            CompletionCount = old.Statistics.CompletionCount,
            PublishDate = old.PublishDate.ToUnixTimeMilliseconds(),
            UpdateDate = old.UpdateDate.ToUnixTimeMilliseconds(),
            MinPlayers = old.MinPlayers,
            MaxPlayers = old.MaxPlayers,
            EnforceMinMaxPlayers = old.EnforceMinMaxPlayers,
            SameScreenGame = old.SameScreenGame,
            BackgroundGuid = old.BackgroundGuid,
            Links = "",
            PhotoCount = old.Statistics.PhotoInLevelCount,
            PublisherPhotoCount = old.Statistics.PhotoByPublisherCount,
        };
        
        if (dataContext.User != null)
        {
            // this is technically invalid, but specifying this for all games ensures they all have the capacity to review if played.
            // we don't store the game's version in play relations, so this is the best we can do
            int plays = dataContext.Database.GetTotalPlaysForLevelByUser(old, dataContext.User);
            response.YourLbp1PlayCount = plays;
            response.YourLbp2PlayCount = plays;
            response.YourLbp3PlayCount = plays;

            if (dataContext.Game is not TokenGame.LittleBigPlanet1 or TokenGame.LittleBigPlanetPSP)
            {
                response.YourReview = SerializedGameReview.FromOld(dataContext.Database.GetReviewByLevelAndUser(old, dataContext.User), dataContext);
            }
        }
        
        if (dataContext.Game is TokenGame.LittleBigPlanetVita or TokenGame.BetaBuild)
        {
            GameAsset? rootResourceAsset = dataContext.Database.GetAssetFromHash(response.RootResource);
            if (rootResourceAsset != null)
            {
                rootResourceAsset.TraverseDependenciesRecursively(dataContext.Database, (_, asset) =>
                {
                    if (asset != null)
                        response.SizeOfResourcesInBytes += asset.SizeInBytes;
                });
            }

            response.SkillRewards = dataContext.Database.GetSkillRewardsForLevel(old).ToList();
        }
        
        return response;
    }

    public static IEnumerable<GameLevelResponse> FromOldList(IEnumerable<GameLevel> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    
    public static GameLevelResponse? FromOld(GamePlaylist? old, DataContext dataContext)
    {
        if (old == null)
            return null;
        
        return new GameLevelResponse
        {
            LevelId = old.PlaylistId,
            IsAdventure = false,
            Title = old.Name,
            IconHash = old.IconHash,
            Description = old.Description,
            Location = new GameLocation(old.LocationX, old.LocationY),
            // Playlists are only ever serialized like this in LBP1-like builds
            GameVersion = TokenGame.LittleBigPlanet1.ToSerializedGame(),
            Type = GameSlotType.Playlist.ToGameType(),
            Handle = SerializedUserHandle.FromUser(old.Publisher, dataContext),
            RootResource = "0",
            PublishDate = old.CreationDate.ToUnixTimeMilliseconds(),
            UpdateDate = old.LastUpdateDate.ToUnixTimeMilliseconds(),
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            HeartCount = 0, 
            TotalPlayCount = 0,
            CompletionCount = 0,
            UniquePlayCount = 0,
            Lbp3PlayCount = 0,
            YourRating = 0,
            YayCount = 0, 
            BooCount = 0,
            YourStarRating = 0,
            YourLbp1PlayCount = 0,
            YourLbp2PlayCount = 0,
            YourLbp3PlayCount = 0, 
            SkillRewards = [],
            TeamPicked = false, 
            XmlResources = [],
            PlayerCount = 0, 
            LevelType = GameLevelType.Normal.ToGameString(),
            IsLocked = false,
            IsSubLevel = false,
            IsCopyable = 0,
            RequiresMoveController = false,
            BackgroundGuid = null,
            Links = null,
            AverageStarRating = 0,
            SizeOfResourcesInBytes = 0,
            ReviewCount = 0,
            ReviewsEnabled = true,
            CommentCount = 0,
            CommentsEnabled = true,
            PhotoCount = 0,
            PublisherPhotoCount = 0,
            Tags = string.Empty,
        };
    }

    public static IEnumerable<GameLevelResponse> FromOldList(IEnumerable<GamePlaylist> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}