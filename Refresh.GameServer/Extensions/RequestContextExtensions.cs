using JetBrains.Annotations;
using Bunkum.Core;

namespace Refresh.GameServer.Extensions;

public static class RequestContextExtensions
{
    [Pure]
    public static (int, int) GetPageData(this RequestContext context, bool api = false, int maxCount = 100)
    {
        int.TryParse(context.QueryString[api ? "skip" : "pageStart"], out int skip);
        if (skip != default) skip--;
        
        int.TryParse(context.QueryString[api ? "count" : "pageSize"], out int count);
        if (count == default) count = 20;

        if (count > maxCount) count = maxCount;

        return (skip, count);
    }

    [Pure]
    public static bool IsPSP(this RequestContext context) => context.RequestHeaders.Get("User-Agent") == "LBPPSP CLIENT";
}