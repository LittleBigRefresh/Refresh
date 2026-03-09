namespace Refresh.Core.RateLimits.Leaderboard;

public static class ScoreListEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int ApiRequestAmount = 70;
    public const int GameRequestAmount = 130; // apparently LBP PSP likes spamming story level leaderboards in certain cases
    public const int BlockDuration = 300;
    public const string ApiRequestBucket = "score-list-api";
    public const string GameRequestBucket = "score-list-game";
}