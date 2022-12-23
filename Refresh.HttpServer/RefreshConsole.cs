using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Refresh.HttpServer;

/// <summary>
/// Helper class for dealing with the console.
/// </summary>
public static partial class RefreshConsole
{
    [LibraryImport("kernel32.dll", EntryPoint = "AllocConsole")]
    private static partial int AllocConsole();

    private static bool _consoleAllocated;

    public static void AllocateConsole()
    {
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            int res = AllocConsole();
            Debug.WriteLine($"{nameof(AllocConsole)} result: {res}");

            _consoleAllocated = res != 0;
        }
        else
        {
            Debug.WriteLine("Not windows, did not need to allocate console");
        }
    }


    /// <summary>
    /// If a console was allocated, wait for a key to be pressed and then exit.
    /// This solves that "flashing" problem where if a console application is double-clicked,
    /// it flashes startup errors leaving no chance for the user to read them.
    /// </summary>
    /// <param name="code">The exit code</param>
    // TODO: Configuration option for advanced users to override this behaviour
    [ContractAnnotation("=> halt")]
    [DoesNotReturn]
    public static void WaitForInputAndExit(int code = 0)
    {
        if (_consoleAllocated) Console.ReadKey();
        Environment.Exit(code);
    }
}