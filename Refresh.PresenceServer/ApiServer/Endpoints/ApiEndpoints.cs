using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Http;

namespace Refresh.PresenceServer.ApiServer.Endpoints;

public class ApiEndpoints : EndpointGroup
{
    [ApiEndpoint("playLevel/{id}", HttpMethods.Post)]
    public Response PlayLevel(RequestContext context, string body, int id) 
        => Program.PresenceServer.PlayLevel(body, id) ? OK : NotFound;
}