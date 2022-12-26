namespace Refresh.HttpServer.Responses;

public enum Method
{
    Get,
    Put,
    Post,
    Delete,
    Head,
    Options,
    Trace,
    Patch,
    Connect,
}

public static class MethodUtils
{
    public static Method FromString(string str)
    {
        return Enum.GetValues<Method>().FirstOrDefault(m => m.ToString().ToUpperInvariant() == str);
    }
}