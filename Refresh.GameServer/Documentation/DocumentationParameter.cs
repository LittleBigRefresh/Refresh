namespace Refresh.GameServer.Documentation;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DocumentationParameter
{
    public DocumentationParameter(string name, ParameterType type, string summary)
    {
        this.Name = name;
        this.Type = type;
        this.Summary = summary;
    }

    public string Name { get; set; }
    public ParameterType Type { get; set; }
    public string Summary { get; set; }
}