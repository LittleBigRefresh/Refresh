namespace Refresh.HttpServer.Endpoints;

[AttributeUsage(AttributeTargets.Method)]
public class AuthenticationAttribute : Attribute
{
    public readonly bool Required;

    public AuthenticationAttribute(bool required)
    {
        this.Required = required;
    }
}