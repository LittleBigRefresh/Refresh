namespace Refresh.Core.RateLimits.Challenges;

public static class ChallengeSingleScoreEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int RequestAmount = 16;
    public const int BlockDuration = 240;
    public const string RequestBucket = "challenge-single-score";
}