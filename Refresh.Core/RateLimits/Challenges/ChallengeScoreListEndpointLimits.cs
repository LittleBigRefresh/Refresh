namespace Refresh.Core.RateLimits.Challenges;

public static class ChallengeScoreListEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 25;
    public const int BlockDuration = 300;
    public const string RequestBucket = "challenge-score-list";
}