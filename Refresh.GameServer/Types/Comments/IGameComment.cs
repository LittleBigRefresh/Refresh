using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Comments;

public interface IGameComment : ISequentialId
{
    GameUser Author { get; set; }
    string Content { get; set; }
    
    long Timestamp { get; set; }
}