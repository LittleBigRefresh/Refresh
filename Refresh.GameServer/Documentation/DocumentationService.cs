using System.Reflection;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

namespace Refresh.GameServer.Documentation;

public class DocumentationService : EndpointService
{
    internal DocumentationService(Logger logger) : base(logger)
    {}

    public override void Initialize()
    {
        AttribDoc.Documentation documentation = this._generator.Document(Assembly.GetExecutingAssembly());
        this._docs.AddRange(ApiRouteResponse.FromOldList(documentation.Routes.OrderBy(r => r.RouteUri)));
    }

    private readonly RefreshDocumentationGenerator _generator = new();
    
    private readonly List<ApiRouteResponse> _docs = new();
    public IEnumerable<ApiRouteResponse> Documentation => this._docs.AsReadOnly();
}