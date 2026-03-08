namespace Refresh.Core.RateLimits.Comment;

public static class CommentUploadEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 18;
    public const int BlockDuration = 300;
    public const string RequestBucket = "comment-upload";
}