using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Comments;

public partial class GameProfileComment : IRealmObject, IGameComment, ISequentialId
{
    [PrimaryKey] public int SequentialId { get; set; }

    /// <inheritdoc/>
    public GameUser Author { get; set; } = null!;

    /// <summary>
    /// The destination profile this comment was posted to.
    /// </summary>
    public GameUser Profile { get; set; } = null!;
    
    /// <inheritdoc/>
    public string Content { get; set; } = string.Empty;
    
    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; set; } 
}