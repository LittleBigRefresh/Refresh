using AttribDoc;
using AttribDoc.Attributes;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Documentation;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class DocumentationApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("documentation"), Authentication(false)]
    [DocSummary("Retrieve a JSON object containing documentation about the API")]
    public ApiListResponse<ApiRouteResponse> GetDocumentation(RequestContext context, DocumentationService service)
    {
        return new ApiListResponse<ApiRouteResponse>(ApiRouteResponse.FromOldList(service.Documentation));
    }
}