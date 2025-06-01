using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.Interfaces.APIv3.Documentation;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;

namespace Refresh.Interfaces.APIv3.Endpoints;

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