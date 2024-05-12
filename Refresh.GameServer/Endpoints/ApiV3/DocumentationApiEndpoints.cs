using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.GameServer.Documentation;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class DocumentationApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("documentation"), Authentication(false)]
    [DocSummary("Retrieve a JSON object containing documentation about the API. You know, the one you're looking at right now.")]
    [ClientCacheResponse(3600)] // 1 hour
    public ApiListResponse<ApiRouteResponse> GetDocumentation(RequestContext context, DocumentationService service)
    {
        return new ApiListResponse<ApiRouteResponse>(service.Documentation);
    }
}