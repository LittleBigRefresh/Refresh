namespace Refresh.Core.RateLimits.Challenges;

public static class ChallengeScoreUploadEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 12;
    public const int BlockDuration = 300;
    public const string RequestBucket = "challenge-score-upload";
}