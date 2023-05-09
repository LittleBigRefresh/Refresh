using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Responses;
using NotEnoughLogs;
using Refresh.GameServer.Services;

namespace Refresh.GameServer.Types.Matching;

public class UpdateMyPlayerDataMethod : IMatchMethod
{
    public Response Execute(MatchService service, LoggerContainer<BunkumContext> logger, SerializedRoomData body)
    {
        return HttpStatusCode.OK;
    }
}