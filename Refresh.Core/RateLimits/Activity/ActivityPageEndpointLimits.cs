namespace Refresh.Core.RateLimits.Activity;

public static class ActivityPageEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int GameRequestAmount = 25;
    public const int ApiRequestAmount = 65;
    public const int BlockDuration = 240;
    public const string GameRequestBucket = "activity-game";
    public const string ApiRequestBucket = "activity-api";

}