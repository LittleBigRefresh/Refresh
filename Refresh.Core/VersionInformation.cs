using System.Reflection;

namespace Refresh.GameServer;

public static class VersionInformation
{
    public static readonly string Version;

    static VersionInformation()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        AssemblyInformationalVersionAttribute? versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        string? version = versionAttribute?.InformationalVersion;
        if(version is null or "0.0.0" or "1.0.0") version = "unknown";

        Version = version;
    }
}