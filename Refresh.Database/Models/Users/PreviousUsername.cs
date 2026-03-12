using MongoDB.Bson;

namespace Refresh.Database.Models.Users;

#nullable disable

[PrimaryKey(nameof(Username), nameof(ReplacedAt))]
public partial class PreviousUsername
{
    public string Username { get; set; }

    [Required]
    public ObjectId UserId { get; set; }

    [Required, ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }

    /// <summary>
    /// When the user's name was changed to no longer use this username
    /// </summary>
    public DateTimeOffset ReplacedAt { get; set; }
}