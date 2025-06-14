using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Comments;

public interface IGameComment
{
    int SequentialId { get; set; }
    
    /// <summary>
    /// The user who originally posted the comment.
    /// </summary>
    #if POSTGRES
    [Required]
    #endif
    GameUser Author { get; set; }
    
    /// <summary>
    /// The text content of the comment.
    /// </summary>
    string Content { get; set; }
    
    /// <summary>
    /// Timestamp in Unix milliseconds
    /// </summary>
    DateTimeOffset Timestamp { get; set; }
}