namespace Refresh.HttpServer.Endpoints;

public class AuthenticationAttribute : Attribute
{
    public readonly bool Required;

    public AuthenticationAttribute(bool required)
    {
        this.Required = required;
    }
}