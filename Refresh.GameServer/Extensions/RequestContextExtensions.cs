using JetBrains.Annotations;
using Bunkum.HttpServer;

namespace Refresh.GameServer.Extensions;

public static class RequestContextExtensions
{
    [Pure]
    public static (int, int) GetPageData(this RequestContext context, bool api = false)
    {
        int.TryParse(context.QueryString[api ? "skip" : "pageStart"], out int skip);
        if (skip != default) skip--;
        
        int.TryParse(context.QueryString[api ? "count" : "pageSize"], out int count);
        if (count == default) count = 20;

        return (skip, count);
    }
}