using System.Net;
using Bunkum.HttpServer.Responses;

namespace Refresh.GameServer.Types.Matching;

public class UpdateMyPlayerDataMethod : IMatchMethod
{
    public Response Execute(string body)
    {
        return HttpStatusCode.OK;
    }
}