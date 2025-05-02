using Refresh.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Comments;

public partial class GameLevelComment : IRealmObject, IGameComment, ISequentialId
{
    [PrimaryKey] public int SequentialId { get; set; }

    /// <inheritdoc/>
    public GameUser Author { get; set; } = null!;

    /// <summary>
    /// The destination level this comment was posted to.
    /// </summary>
    public GameLevel Level { get; set; } = null!;
    
    /// <inheritdoc/>
    public string Content { get; set; } = string.Empty;
    
    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; set; }
}