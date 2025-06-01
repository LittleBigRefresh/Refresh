namespace Refresh.Core.Configuration;

public class TimedLevelUploadLimitProperties
{
    public TimedLevelUploadLimitProperties() {}

    /// <summary>
    /// Whether to enable the timed level uploading limits
    /// </summary>
    public bool Enabled { get; set; }
    /// <summary>
    /// The amount of time until level upload counts are reset in hours
    /// </summary>
    public int TimeSpanHours { get; set; }
    /// <summary>
    /// The amount of levels the user is allowed to upload during the configured time span before level uploads are blocked
    /// </summary>
    public int LevelQuota { get; set; }
}