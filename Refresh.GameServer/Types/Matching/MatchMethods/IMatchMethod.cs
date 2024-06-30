using Bunkum.Core.Responses;
using JetBrains.Annotations;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMatchMethod
{
    IEnumerable<string> MethodNames { get; }

    Response Execute(MatchService service, Logger logger,
        GameDatabaseContext database, GameUser user, Token token, SerializedRoomData body,
        GameServerConfig gameServerConfig);
}