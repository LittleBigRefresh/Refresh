using JetBrains.Annotations;
using Refresh.HttpServer;

namespace Refresh.GameServer.Extensions;

public static class RequestContextExtensions
{
    [Pure]
    public static (int, int) GetPageData(this RequestContext context)
    {
        int.TryParse(context.Request.QueryString["pageStart"], out int skip);
        if (skip != default) skip--;
        
        int.TryParse(context.Request.QueryString["pageSize"], out int count);
        if (count == default) count = 20;

        return (skip, count);
    }
}