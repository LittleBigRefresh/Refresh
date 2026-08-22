namespace Refresh.Core.Configuration;

public class ConfigRateLimitBucket
{
    public int WindowDurationSeconds { get; set; }
    public int MaxRequestCount { get; set; }
    public int InitialBlockDurationSeconds { get; set; }

    public ConfigRateLimitBucket(int windowDurationSeconds, int maxRequestCount, int initialBlockDurationSeconds)
    {
        this.WindowDurationSeconds = windowDurationSeconds;
        this.MaxRequestCount = maxRequestCount;
        this.InitialBlockDurationSeconds = initialBlockDurationSeconds;
    }
}