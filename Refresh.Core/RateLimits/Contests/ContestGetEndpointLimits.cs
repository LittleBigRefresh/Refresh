namespace Refresh.Core.RateLimits.Contests;

// treat getting one and getting a list as the same for now
public static class ContestGetEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 10;
    public const int BlockDuration = 300;
    public const string RequestBucket = "contests";
}