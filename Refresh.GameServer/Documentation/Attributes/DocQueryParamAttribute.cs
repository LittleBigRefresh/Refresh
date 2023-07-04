namespace Refresh.GameServer.Documentation.Attributes;

public class DocQueryParamAttribute : Attribute
{
    public DocQueryParamAttribute(string name, string summary)
    {
        this.Name = name;
        this.Summary = summary;
    }

    public string Name { get; private init; }
    public string Summary { get; private init; }
}