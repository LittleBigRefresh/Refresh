using System.Reflection;

namespace Refresh.GameServer;

internal static class VersionInformation
{
    internal static readonly string Version;

    static VersionInformation()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        AssemblyInformationalVersionAttribute? versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        string? version = versionAttribute?.InformationalVersion;
        if(version is null or "1.0.0") version = "unknown";

        Version = version;
    }
}