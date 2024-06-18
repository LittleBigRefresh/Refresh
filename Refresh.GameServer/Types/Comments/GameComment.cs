using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Refresh.GameServer.Types.Comments;

public partial class GameComment : IRealmObject, ISequentialId
{
    [PrimaryKey] public int SequentialId { get; set; }

    public GameUser Author { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp in Unix milliseconds
    /// </summary>
    public long Timestamp { get; set; } 
}
