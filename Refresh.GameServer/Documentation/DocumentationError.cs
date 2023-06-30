namespace Refresh.GameServer.Documentation;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DocumentationError
{
    public DocumentationError(string name, string occursWhen)
    {
        this.Name = name;
        this.OccursWhen = occursWhen;
    }

    public string Name { get; set; }
    public string OccursWhen { get; set; }
}