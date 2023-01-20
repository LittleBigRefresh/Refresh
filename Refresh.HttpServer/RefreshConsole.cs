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
    
    [LibraryImport("kernel32.dll", EntryPoint = "GetConsoleProcessList")]
    private static partial uint GetConsoleProcessList(uint[] processList, uint processCount);

    public static void AllocateConsole()
    {
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            int res = AllocConsole();
            Debug.WriteLine($"{nameof(AllocConsole)} result: {res}");
        }
        else
        {
            Debug.WriteLine("Not windows, did not need to allocate console");
        }
    }
    
    /// <summary>
    /// Determines if the console will be destroyed after execution.
    /// https://devblogs.microsoft.com/oldnewthing/20160125-00/?p=92922
    /// </summary>
    private static readonly Lazy<bool> WillConsoleBeDestroyed = new(() =>
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false;

        const uint idCount = 16;
        
        uint[] ids = new uint[idCount];
        uint count = GetConsoleProcessList(ids, idCount);
        
        return count <= 2;
    });

    /// <summary>
    /// If a console was allocated, wait for a key to be pressed and then exit.
    /// This solves that "flashing" problem where if a console application is double-clicked,
    /// it flashes startup errors leaving no chance for the user to read them.
    /// </summary>
    /// <param name="code">The exit code</param>
    [ContractAnnotation("=> halt")]
    [DoesNotReturn]
    public static void WaitForInputAndExit(int code = 0)
    {
        bool overridePause = false;
        
        try
        {
            // When exiting from an exception, the default Refresh behavior on Windows is to pause before exiting if
            // the console session is going to be destroyed. Setting this to true will override this behavior.
            // Turn this on if you are hosting from inside a utility like screen/tmux and are having issues.
            overridePause = Convert.ToBoolean(Environment.GetEnvironmentVariable("REFRESH_OVERRIDE_PAUSE_ON_INTERRUPTION"));
        }
        catch
        {
            // ignored
        }
        
        if (!overridePause && WillConsoleBeDestroyed.Value)
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        
        Environment.Exit(code);
    }
}