using Bunkum.Core.Database;
using Refresh.GameServer.Time;

namespace Refresh.GameServer.Database;

public interface IGameDatabaseProvider
{
    protected IDateTimeProvider Time { get; }

    public IGameDatabaseContext GetContext();
}