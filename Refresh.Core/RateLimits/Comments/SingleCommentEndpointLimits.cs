namespace Refresh.Core.RateLimits.Comment;

public static class SingleCommentEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 12;
    public const int BlockDuration = 300;
    public const string RequestBucket = "single-comment";
}