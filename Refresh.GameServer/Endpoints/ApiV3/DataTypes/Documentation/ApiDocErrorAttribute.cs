namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Documentation;

public class ApiDocErrorAttribute : Attribute
{
    public ApiDocErrorAttribute(Type errorType, string when)
    {
        this.ErrorType = errorType;
        this.When = when;
    }

    public Type ErrorType { get; private init; }
    public string When { get; private init; }
}