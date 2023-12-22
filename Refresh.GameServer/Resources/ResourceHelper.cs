using System.Reflection;

namespace Refresh.GameServer.Resources;

public static class ResourceHelper
{
    public static Stream StreamFromResource(string name)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(name)!;
    }   
}