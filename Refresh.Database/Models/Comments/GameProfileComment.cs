using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Comments;

#nullable disable

public partial class GameProfileComment : IGameComment, ISequentialId
{
    [Key] public int SequentialId { get; set; }

    /// <inheritdoc/>
    [Required, ForeignKey(nameof(AuthorUserId))] public GameUser Author { get; set; } = null!;
    [Required] public ObjectId AuthorUserId { get; set; }

    /// <summary>
    /// The destination profile this comment was posted to.
    /// </summary>
    [Required]
    public GameUser Profile { get; set; } = null!;
    
    /// <inheritdoc/>
    public string Content { get; set; } = string.Empty;
    
    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; set; } 
}