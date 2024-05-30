using System.Collections.Frozen;
using System.Reflection;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Data;

namespace Refresh.GameServer.Documentation;

public class DocumentationService : EndpointService
{
    internal DocumentationService(Logger logger) : base(logger)
    {}

    public override void Initialize()
    {
        List<ApiRouteResponse> docs = [];
        
        AttribDoc.Documentation documentation = this._generator.Document(Assembly.GetExecutingAssembly());
        docs.AddRange(ApiRouteResponse.FromOldList(documentation.Routes.OrderBy(r => r.RouteUri)));

        this.Documentation = docs.ToFrozenSet();
    }

    private readonly RefreshDocumentationGenerator _generator = new();
    public FrozenSet<ApiRouteResponse> Documentation { get; private set; } = null!;
}