using Bunkum.HttpServer;
using Bunkum.HttpServer.Responses;
using JetBrains.Annotations;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMatchMethod
{
    IEnumerable<string> MethodNames { get; }

    Response Execute(MatchService service, LoggerContainer<BunkumContext> logger,
        GameDatabaseContext database, GameUser user, SerializedRoomData body);
}