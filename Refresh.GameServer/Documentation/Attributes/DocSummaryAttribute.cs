namespace Refresh.GameServer.Documentation.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class DocSummaryAttribute : Attribute
{
    public DocSummaryAttribute(string summary)
    {
        this.Summary = summary;
    }

    public string Summary { get; private init; }
}