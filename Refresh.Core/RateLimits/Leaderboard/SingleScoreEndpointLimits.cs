namespace Refresh.Core.RateLimits.Leaderboard;

public static class SingleScoreEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 30;
    public const int BlockDuration = 300;
    public const string RequestBucket = "single-score";
}