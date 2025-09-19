using System.Diagnostics;
using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Reviews;

namespace Refresh.Interfaces.Game.Endpoints.DataTypes.Response;

[XmlRoot("slot")]
[XmlType("slot")]
public class GameLevelResponse : GameMinimalLevelResponse, IDataConvertableFrom<GameLevelResponse, GameLevel>, IDataConvertableFrom<GameLevelResponse, GamePlaylist>
{
    [XmlElement("firstPublished")] public long PublishDate { get; set; } // unix seconds
    [XmlElement("lastUpdated")] public long UpdateDate { get; set; }
    [XmlElement("enforceMinMaxPlayers")] public bool EnforceMinMaxPlayers { get; set; }
    [XmlElement("sameScreenGame")] public bool SameScreenGame { get; set; }
    [XmlElement("completionCount")] public int CompletionCount { get; set; }

    [XmlElement("yourlbp1PlayCount")] public int YourLbp1PlayCount { get; set; }
    [XmlElement("yourlbp2PlayCount")] public int YourLbp2PlayCount { get; set; }
    [XmlElement("yourlbp3PlayCount")] public int YourLbp3PlayCount { get; set; }

    [XmlArray("customRewards")]
    [XmlArrayItem("customReward")]
    public List<GameSkillReward> SkillRewards { get; set; } = [];

    [XmlElement("resource")] public List<string> XmlResources { get; set; } = new();
    [XmlElement("backgroundGUID")] public string? BackgroundGuid { get; set; }
    [XmlElement("links")] public string? Links { get; set; }
    [XmlElement("sizeOfResources")] public int SizeOfResourcesInBytes { get; set; }

    [XmlElement("yourReview")] public SerializedGameReview? YourReview { get; set; }
    [XmlElement("photoCount")] public int PhotoCount { get; set; }
    [XmlElement("authorPhotoCount")] public int PublisherPhotoCount { get; set; }

    private static GameLevelResponse FromMinimal(GameMinimalLevelResponse minimal)
    {
        return new()
        {
            LevelId = minimal.LevelId,
            IsAdventure = minimal.IsAdventure,
            Title = minimal.Title,
            IconHash = minimal.IconHash,
            GameVersion = minimal.GameVersion,
            RootResource = minimal.RootResource,
            Description = minimal.Description,
            Location = minimal.Location,
            Handle = minimal.Handle,
            Type = minimal.Type,
            TeamPicked = minimal.TeamPicked,
            MinPlayers = minimal.MinPlayers,
            MaxPlayers = minimal.MaxPlayers,
            HeartCount = minimal.HeartCount,
            TotalPlayCount = minimal.TotalPlayCount,
            UniquePlayCount = minimal.UniquePlayCount,
            Lbp3PlayCount = minimal.Lbp3PlayCount,
            YayCount = minimal.YayCount,
            BooCount = minimal.BooCount,
            AverageStarRating = minimal.AverageStarRating,
            YourStarRating = minimal.YourStarRating,
            YourRating = minimal.YourRating,
            PlayerCount = minimal.PlayerCount,
            ReviewsEnabled = minimal.ReviewsEnabled,
            ReviewCount = minimal.ReviewCount,
            CommentsEnabled = minimal.CommentsEnabled,
            CommentCount = minimal.CommentCount,
            IsLocked = minimal.IsLocked,
            IsSubLevel = minimal.IsSubLevel,
            IsCopyable = minimal.IsCopyable,
            RequiresMoveController = minimal.RequiresMoveController,
            LevelType = minimal.LevelType,
            Tags = minimal.Tags,
            PublisherLabels = minimal.PublisherLabels,
            AllLabels = minimal.AllLabels,
        };
    }
    
    public static new GameLevelResponse FromHash(string hash, DataContext dataContext)
    {
        GameLevelResponse response = FromMinimal(GameMinimalLevelResponse.FromHash(hash, dataContext));

        response.CompletionCount = 0;
        response.PublishDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        response.UpdateDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        response.MinPlayers = 1;
        response.MaxPlayers = 4;
        response.EnforceMinMaxPlayers = false;
        response.SameScreenGame = false;
        response.Links = "";
        response.PhotoCount = 0;
        response.PublisherPhotoCount = 0;

        return response;
    }

    public static new GameLevelResponse? FromOld(GameLevel? old, DataContext dataContext)
    {
        GameMinimalLevelResponse? minimal = GameMinimalLevelResponse.FromOld(old, dataContext);
        if (minimal == null) return null;

        GameLevelResponse response = FromMinimal(minimal);
        Debug.Assert(old?.Statistics != null);

        response.CompletionCount = old.Statistics.CompletionCount;
        response.PublishDate = old.PublishDate.ToUnixTimeMilliseconds();
        response.UpdateDate = old.UpdateDate.ToUnixTimeMilliseconds();
        response.MinPlayers = old.MinPlayers;
        response.MaxPlayers = old.MaxPlayers;
        response.EnforceMinMaxPlayers = old.EnforceMinMaxPlayers;
        response.SameScreenGame = old.SameScreenGame;
        response.BackgroundGuid = old.BackgroundGuid;
        response.Links = "";
        response.PhotoCount = old.Statistics.PhotoInLevelCount;
        response.PublisherPhotoCount = old.Statistics.PhotoByPublisherCount;
        
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

    public static new IEnumerable<GameLevelResponse> FromOldList(IEnumerable<GameLevel> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    
    public static new GameLevelResponse? FromOld(GamePlaylist? old, DataContext dataContext)
    {
        GameMinimalLevelResponse? minimal = GameMinimalLevelResponse.FromOld(old, dataContext);
        if (minimal == null) return null;

        GameLevelResponse response = FromMinimal(minimal);
        Debug.Assert(old?.Statistics != null);

        response.CompletionCount = 0;
        response.PublishDate = old.CreationDate.ToUnixTimeMilliseconds();
        response.UpdateDate = old.LastUpdateDate.ToUnixTimeMilliseconds();
        response.MinPlayers = 0;
        response.MaxPlayers = 0;
        response.EnforceMinMaxPlayers = false;
        response.SameScreenGame = false;
        response.Links = "";
        response.PhotoCount = 0;
        response.PublisherPhotoCount = 0;

        return response;
    }

    public static new IEnumerable<GameLevelResponse> FromOldList(IEnumerable<GamePlaylist> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}