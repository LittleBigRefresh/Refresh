using Bunkum.Core.Database;
using Refresh.GameServer.Time;

namespace Refresh.GameServer.Database;

public interface IGameDatabaseProvider : IDatabaseProvider<GameDatabaseContext>
{
    protected IDateTimeProvider Time { get; }

    public GameDatabaseContext GetContext();
}