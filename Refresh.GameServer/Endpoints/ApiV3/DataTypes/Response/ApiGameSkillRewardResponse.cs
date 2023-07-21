using Refresh.GameServer.Types.Levels.SkillRewards;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

public class ApiGameSkillRewardResponse
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public string? Title { get; set; }
    public float RequiredAmount { get; set; }
    public GameSkillRewardCondition ConditionType { get; set; }

    public static ApiGameSkillRewardResponse FromNonResponse(GameSkillReward reward)
    {
        return new ApiGameSkillRewardResponse
        {
            Id = reward.Id,
            Enabled = reward.Enabled,
            Title = reward.Title,
            RequiredAmount = reward.RequiredAmount,
            ConditionType = reward.ConditionType,
        };
    }
}