namespace Refresh.Core.RateLimits.Leaderboard;

public static class ScoreListEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 90;
    public const int BlockDuration = 300;
    public const string RequestBucket = "score-list";
}