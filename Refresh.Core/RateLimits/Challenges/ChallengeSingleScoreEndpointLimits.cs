namespace Refresh.Core.RateLimits.Challenges;

public static class ChallengeSingleScoreEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 18;
    public const int BlockDuration = 300;
    public const string RequestBucket = "challenge-single-score";
}