using System.Text;
using NotEnoughLogs;
using NotEnoughLogs.Sinks;

namespace RefreshTests.GameServer.Logging;

public class NUnitSink : ILoggerSink
{
    public void Log(LogLevel level, ReadOnlySpan<char> category, ReadOnlySpan<char> content)
    {
        TextWriter stream = level switch
        {
            LogLevel.Critical => NUnit.Framework.TestContext.Error,
            LogLevel.Error => NUnit.Framework.TestContext.Error,
            LogLevel.Warning => NUnit.Framework.TestContext.Progress,
            LogLevel.Info => NUnit.Framework.TestContext.Progress,
            LogLevel.Debug => NUnit.Framework.TestContext.Progress,
            LogLevel.Trace => NUnit.Framework.TestContext.Progress,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
        
        StringBuilder iHateMicrosoftApis = new();
        iHateMicrosoftApis.Append('[');
        iHateMicrosoftApis.Append(level.ToString());
        iHateMicrosoftApis.Append(']');
        iHateMicrosoftApis.Append(' ');
            
        iHateMicrosoftApis.Append('[');
        iHateMicrosoftApis.Append(category);
        iHateMicrosoftApis.Append(']');
        iHateMicrosoftApis.Append(' ');
            
        iHateMicrosoftApis.Append(content);

        stream.WriteLine(iHateMicrosoftApis.ToString());
    }

    public void Log(LogLevel level, ReadOnlySpan<char> category, ReadOnlySpan<char> format, params object[] args)
    {
        this.Log(level, category, string.Format(format.ToString(), args));
    }
}