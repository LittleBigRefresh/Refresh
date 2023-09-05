using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameLevelResponse : IApiResponse, IDataConvertableFrom<ApiGameLevelResponse, GameLevel>
{
    public required int LevelId { get; set; }
    public required ApiGameUserResponse? Publisher { get; set; }

    public required string Title { get; set; }
    public required string IconHash { get; set; }
    public required string Description { get; set; }
    public required ApiGameLocationResponse Location { get; set; }
    
    public required string RootLevelHash { get; set; }
    public TokenGame GameVersion { get; set; }
    
    public required DateTimeOffset PublishDate { get; set; }
    public required DateTimeOffset UpdateDate { get; set; }
    
    public required int MinPlayers { get; set; }
    public required int MaxPlayers { get; set; }
    public required bool EnforceMinMaxPlayers { get; set; }
    
    public required bool SameScreenGame { get; set; }
    
    public required IEnumerable<ApiGameSkillRewardResponse>? SkillRewards { get; set; }
    
    public required int YayRatings { get; set; }
    public required int BooRatings { get; set; }
    public required int Hearts { get; set; }
    public required int UniquePlays { get; set; }
    public required bool TeamPicked { get; set; }
    public required GameLevelType LevelType { get; set; }
    public required bool IsLocked { get; set; }
    public required bool IsSubLevel { get; set; }
    public required bool IsCopyable { get; set; }

    public static ApiGameLevelResponse? FromOld(GameLevel? level)
    {
        if (level == null) return null;
        
        return new ApiGameLevelResponse
        {
            Title = level.Title,
            Publisher = ApiGameUserResponse.FromOld(level.Publisher),
            LevelId = level.LevelId,
            IconHash = level.IconHash,
            Description = level.Description,
            Location = ApiGameLocationResponse.FromGameLocation(level.Location)!,
            PublishDate = DateTimeOffset.FromUnixTimeMilliseconds(level.PublishDate),
            UpdateDate = DateTimeOffset.FromUnixTimeMilliseconds(level.UpdateDate),
            MinPlayers = level.MinPlayers,
            MaxPlayers = level.MaxPlayers,
            EnforceMinMaxPlayers = level.EnforceMinMaxPlayers,
            SameScreenGame = level.SameScreenGame,
            SkillRewards = ApiGameSkillRewardResponse.FromOldList(level.SkillRewards),
            YayRatings = level.Ratings.Count(r => r._RatingType == (int)RatingType.Yay),
            BooRatings = level.Ratings.Count(r => r._RatingType == (int)RatingType.Boo),
            Hearts = level.FavouriteRelations.Count(),
            UniquePlays = level.UniquePlays.Count(),
            TeamPicked = level.TeamPicked,
            RootLevelHash = level.RootResource,
            GameVersion = level.GameVersion,
            LevelType = level.LevelType,
            IsCopyable = level.IsCopyable,
            IsLocked = level.IsLocked,
            IsSubLevel = level.IsSubLevel,
        };
    }

    public static IEnumerable<ApiGameLevelResponse> FromOldList(IEnumerable<GameLevel> oldList) => oldList.Select(FromOld)!;
}