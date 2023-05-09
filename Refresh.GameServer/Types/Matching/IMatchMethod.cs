using Bunkum.HttpServer;
using Bunkum.HttpServer.Responses;
using JetBrains.Annotations;
using NotEnoughLogs;
using Refresh.GameServer.Services;

namespace Refresh.GameServer.Types.Matching;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMatchMethod
{
    Response Execute(MatchService service, LoggerContainer<BunkumContext> logger, SerializedRoomData body);
}