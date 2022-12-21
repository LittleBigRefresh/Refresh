using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Refresh.HttpServer;

/// <summary>
/// Helper class for allocating a console.
/// </summary>
public static partial class RefreshConsole
{
    [LibraryImport("kernel32.dll", EntryPoint = "AllocConsole")]
    private static partial int AllocConsole();

    public static void AllocateConsole()
    {
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Debug.WriteLine($"{nameof(AllocConsole)} result: {AllocConsole()}");
        }
        else
        {
            Debug.WriteLine("Not windows, did not need to allocate console");
        }
    }
}