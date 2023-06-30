namespace Refresh.GameServer.Documentation.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class DocErrorAttribute : Attribute
{
    public DocErrorAttribute(Type errorType, string when)
    {
        this.ErrorType = errorType;
        this.When = when;
    }

    public Type ErrorType { get; private init; }
    public string When { get; private init; }
}