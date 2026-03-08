namespace Refresh.Core.RateLimits.Comments;

public static class CommentListEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 25;
    public const int BlockDuration = 300;
    public const string RequestBucket = "comment-list";
}