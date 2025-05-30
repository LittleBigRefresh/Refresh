using System.Net;
using JetBrains.Annotations;
using Bunkum.Core;

namespace Refresh.GameServer.Extensions;

public static class RequestContextExtensions
{
    [Pure]
    public static (int, int) GetPageData(this RequestContext context, int maxCount = 100)
    {
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

    [Pure]
    public static bool IsPatchworkVersionValid(this RequestContext context, int requiredMajor, int requiredMinor)
    {
        ReadOnlySpan<char> userAgent = context.RequestHeaders.Get("User-Agent").AsSpan();
        if (userAgent.IsEmpty)
            return false;

        return IsPatchworkVersionValid(userAgent, requiredMajor, requiredMinor);
    }

    [Pure]
    public static bool IsPatchworkVersionValid(ReadOnlySpan<char> userAgent, int requiredMajor, int requiredMinor)
    {
        // example: PatchworkLBP2 1.0
        const string namePrefix = "PatchworkLBP";

        // does the useragent even match patchwork's name?
        if (!userAgent.StartsWith(namePrefix))
            return false;
        
        // is the game version valid?
        char gameVersion = userAgent[namePrefix.Length];
        if (gameVersion is not '1' and not '2' and not '3' and not 'V')
            return false;

        ReadOnlySpan<char> versionString = userAgent[(namePrefix.Length + 2)..];
        if (gameVersion == 'V') // HTTP library on vita adds extra data, handle that scenario here
        {
            int spaceIndex = versionString.IndexOf(' ');
            if (spaceIndex != -1)
                versionString = versionString[..spaceIndex];
        }

        // does the version string parse out?
        if (!Version.TryParse(versionString, out Version? version))
            return false;

        // are we on a supported version?
        if (version.Major < requiredMajor || version.Minor < requiredMinor)
            return false;

        // if everything held up, this client is up to date on security patches!
        return true;
    }
    
    [Pure]
    public static string RemoteIp(this RequestContext context) => ((IPEndPoint)context.RemoteEndpoint).Address.ToString();
}