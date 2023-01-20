using System.ComponentModel;

namespace Refresh.HttpServer.Configuration;

public interface IConfig
{
    /// <summary>
    /// When exiting from an exception, the default Refresh behavior on Windows is to pause before exiting if the console session is going to be destroyed.
    /// Setting this to true will override this behavior.
    /// Turn this on if you are hosting from inside a utility like screen/tmux and are having issues.
    /// </summary>
    /// <seealso cref="RefreshConsole"/>
    [DefaultValue(false)]
    public bool RefreshOverridePauseOnInterruption { get; }
}