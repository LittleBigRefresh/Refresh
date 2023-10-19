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

        lock (stream)
        {
            stream.Write('[');
            stream.Write(level.ToString());
            stream.Write(']');
            stream.Write(' ');
            
            stream.Write('[');
            stream.Write(category);
            stream.Write(']');
            stream.Write(' ');
            
            stream.WriteLine(content);
        }
    }

    public void Log(LogLevel level, ReadOnlySpan<char> category, ReadOnlySpan<char> format, params object[] args)
    {
        this.Log(level, category, string.Format(format.ToString(), args));
    }
}