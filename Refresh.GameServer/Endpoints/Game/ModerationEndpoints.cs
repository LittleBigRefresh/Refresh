using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ModerationEndpoints : EndpointGroup
{
    [GameEndpoint("showModeratedSlots", Method.Post, ContentType.Xml)]
    public SerializedModeratedSlotList ModerateSlots(RequestContext context, SerializedModeratedSlotList body)
    {
        return new SerializedModeratedSlotList
        {
            Ids = new List<int>(),
        };
    }
}