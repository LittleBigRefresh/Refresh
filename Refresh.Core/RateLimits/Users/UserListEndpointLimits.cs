namespace Refresh.Core.RateLimits.Users;

public static class UserListEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 50;
    public const int BlockDuration = 300;
    public const string ApiRequestBucket = "user-list";
}