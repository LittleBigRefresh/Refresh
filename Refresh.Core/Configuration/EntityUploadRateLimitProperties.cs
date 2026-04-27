namespace Refresh.Core.Configuration;

public class EntityUploadRateLimitProperties
{
    public EntityUploadRateLimitProperties() {}

    /// <summary>
    /// Whether to rate-limit uploads of a certain entity (level/photo/playlist) using the database
    /// </summary>
    public bool Enabled { get; set; }
    /// <summary>
    /// The duration of this rate-limit
    /// </summary>
    public int TimeSpanHours { get; set; }
    /// <summary>
    /// The amount of entities the user is allowed to upload during the specified time span
    /// </summary>
    public int UploadQuota { get; set; }
}