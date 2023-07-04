using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Documentation;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class DocumentationApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("documentation"), Authentication(false)]
    [DocSummary("Retrieve a JSON object containing documentation about the API")]
    public ApiListResponse<DocumentationRoute> GetDocumentation(RequestContext context)
    {
        return new ApiListResponse<DocumentationRoute>(DocumentationHelper.Documentation);
    }
}