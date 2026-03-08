namespace Refresh.Core.RateLimits.Levels;

public static class SingleLevelEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int ApiRequestAmount = 50;
    public const int GameRequestAmount = 170; // Game likes to request them a lot
    public const int BlockDuration = 240;
    public const string ApiRequestBucket = "single-level-api";
    public const string GameRequestBucket = "single-level-game";
}