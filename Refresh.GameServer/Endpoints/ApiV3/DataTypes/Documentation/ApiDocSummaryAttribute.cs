namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Documentation;

[AttributeUsage(AttributeTargets.Method)]
public class ApiDocSummaryAttribute : Attribute
{
    public ApiDocSummaryAttribute(string summary)
    {
        this.Summary = summary;
    }

    public string Summary { get; private init; }
}