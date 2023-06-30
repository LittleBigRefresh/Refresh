namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Documentation;

[AttributeUsage(AttributeTargets.Parameter)]
public class ApiDocParamAttribute : Attribute
{
    public ApiDocParamAttribute(string summary)
    {
        this.Summary = summary;
    }

    public string Summary { get; private init; }
}