namespace Refresh.GameServer.Documentation;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DocumentationRoute
{
    public DocumentationRoute(string route, string summary)
    {
        this.Route = route;
        this.Summary = summary;
    }

    public string Route { get; set; }
    public string Summary { get; set; }
    
    public bool AuthenticationRequired { get; set; }

    public List<DocumentationParameter> Parameters { get; set; } = new();
    public List<DocumentationError> PotentialErrors { get; set; } = new();
}