namespace Refresh.Core.RateLimits.Challenges;

public static class ChallengeListEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 20;
    public const int BlockDuration = 300;
    public const string RequestBucket = "challenge-list";
}