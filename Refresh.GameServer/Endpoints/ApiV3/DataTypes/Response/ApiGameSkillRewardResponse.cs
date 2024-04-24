using Refresh.GameServer.Types.Levels.SkillRewards;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameSkillRewardResponse : IApiResponse
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public string? Title { get; set; }
    public float RequiredAmount { get; set; }
    public GameSkillRewardCondition ConditionType { get; set; }

    public static ApiGameSkillRewardResponse? FromOld(GameSkillReward? old)
    {
        if (old == null) return null;
        
        return new ApiGameSkillRewardResponse
        {
            Id = old.Id,
            Enabled = old.Enabled,
            Title = old.Title,
            RequiredAmount = old.RequiredAmount,
            ConditionType = old.ConditionType,
        };
    }

    public static IEnumerable<ApiGameSkillRewardResponse> FromOldList(IEnumerable<GameSkillReward> oldList) => oldList.Select(FromOld).ToList()!;
}