namespace Refresh.GameServer.Documentation;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DocumentationParameter
{
    public DocumentationParameter(string name, string summary)
    {
        this.Name = name;
        this.Summary = summary;
    }

    public string Name { get; set; }
    public string Summary { get; set; }
}