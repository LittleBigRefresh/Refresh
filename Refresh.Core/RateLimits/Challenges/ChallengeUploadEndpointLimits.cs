namespace Refresh.Core.RateLimits.Challenges;

public static class ChallengeUploadEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 10;
    public const int BlockDuration = 300;
    public const string RequestBucket = "challenge-upload";
}