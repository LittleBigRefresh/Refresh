using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Newtonsoft.Json;
using Refresh.GameServer.Types.UserData.Photos;

namespace Refresh.GameServer.Endpoints.Game;

public class PhotoEndpoints : EndpointGroup
{
    [GameEndpoint("uploadPhoto", Method.Post, ContentType.Xml)]
    public Response UploadPhoto(RequestContext context, SerializedPhoto body)
    {
        Console.WriteLine(JsonConvert.SerializeObject(body));
        return NotFound;
    }
}