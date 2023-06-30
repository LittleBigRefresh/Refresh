namespace Refresh.GameServer.Documentation.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class DocParamAttribute : Attribute
{
    public DocParamAttribute(string summary)
    {
        this.Summary = summary;
    }

    public string Summary { get; private init; }
}