using Refresh.GameServer.Endpoints.ApiV3.DataTypes;

namespace Refresh.GameServer.Documentation;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DocumentationRoute : IApiResponse
{
    public DocumentationRoute(string method, string route, string summary)
    {
        this.Method = method;
        this.Route = route;
        this.Summary = summary;
    }

    public string Method { get; set; }
    public string Route { get; set; }
    public string Summary { get; set; }
    
    public bool AuthenticationRequired { get; set; }

    public List<DocumentationParameter> Parameters { get; set; } = new();
    public List<DocumentationError> PotentialErrors { get; set; } = new();
}