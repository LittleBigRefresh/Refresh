using JetBrains.Annotations;
using Bunkum.Core;

namespace Refresh.GameServer.Extensions;

public static class RequestContextExtensions
{
    [Pure]
    public static (int, int) GetPageData(this RequestContext context)
    {
        const int maxCount = 100;
        bool api = context.IsApi();
        
        bool parsed = int.TryParse(context.QueryString[api ? "skip" : "pageStart"], out int skip);
        if (parsed) skip--; // If we parsed, subtract the number of items to skip by one to prevent an off-by-one.
        
        parsed = int.TryParse(context.QueryString[api ? "count" : "pageSize"], out int count);
        if (!parsed) count = 20; // Default items in a page
        
        count = Math.Clamp(count, 0, maxCount);

        return (skip, count);
    }

    [Pure]
    public static bool IsPSP(this RequestContext context) => context.RequestHeaders.Get("User-Agent") == "LBPPSP CLIENT";

    [Pure]
    public static bool IsApi(this RequestContext context) => context.Url.AbsolutePath.StartsWith("/api/");
}