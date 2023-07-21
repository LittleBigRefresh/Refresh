using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameLevelResponse : IApiResponse, IDataConvertableFrom<ApiGameLevelResponse, GameLevel>
{
    [JsonProperty] public required int LevelId { get; set; }
    [JsonProperty] public required ApiGameUserResponse? Publisher { get; set; }

    [JsonProperty] public required string Title { get; set; }
    [JsonProperty] public required string IconHash { get; set; }
    [JsonProperty] public required string Description { get; set; }
    [JsonProperty] public required ApiGameLocationResponse Location { get; set; }
    
    [JsonProperty] public required DateTimeOffset PublishDate { get; set; }
    [JsonProperty] public required DateTimeOffset UpdateDate { get; set; }
    
    [JsonProperty] public int MinPlayers { get; set; }
    [JsonProperty] public int MaxPlayers { get; set; }
    [JsonProperty] public bool EnforceMinMaxPlayers { get; set; }
    
    [JsonProperty] public bool SameScreenGame { get; set; }
    
    [JsonProperty] public IEnumerable<ApiGameSkillRewardResponse>? SkillRewards { get; set; }

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
        };
    }

    public static IEnumerable<ApiGameLevelResponse> FromOldList(IEnumerable<GameLevel> oldList) => oldList.Select(FromOld)!;
}