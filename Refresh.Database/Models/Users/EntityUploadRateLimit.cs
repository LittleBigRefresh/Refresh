using MongoDB.Bson;

namespace Refresh.Database.Models.Users;

#nullable disable

[PrimaryKey(nameof(UserId), nameof(Entity))]
public partial class EntityUploadRateLimit
{
    public GameDatabaseEntity Entity { get; set; }

    [Required]
    public ObjectId UserId { get; set; }

    [Required, ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }

    /// <summary>
    /// How many entites of the specified type the user has uploaded during the rate-limit
    /// </summary>
    public int UploadCount { get; set; }
    /// <summary>
    /// When this rate-limit should be expired
    /// </summary>
    public DateTimeOffset ExpiryDate { get; set; }
}