using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Comments;

public interface IGameComment
{
    int SequentialId { get; set; }
    
    /// <summary>
    /// The user who originally posted the comment.
    /// </summary>
    GameUser Author { get; set; }
    
    /// <summary>
    /// The text content of the comment.
    /// </summary>
    string Content { get; set; }
    
    /// <summary>
    /// Timestamp in Unix milliseconds
    /// </summary>
    long Timestamp { get; set; }
}