namespace Refresh.Core.Configuration;

public class ConfigRateLimitBucket
{
    public int TimeWindowSeconds { get; set; }
    public int MaxRequestAmount { get; set; }
    public int BlockDurationSeconds { get; set; }

    public ConfigRateLimitBucket(int windowDurationSeconds, int maxRequestAmount, int blockDurationSeconds)
    {
        this.TimeWindowSeconds = windowDurationSeconds;
        this.MaxRequestAmount = maxRequestAmount;
        this.BlockDurationSeconds = blockDurationSeconds;
    }
}