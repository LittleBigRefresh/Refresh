using System.Globalization;
using System.Reflection;
using Bunkum.Core;
using Bunkum.Protocols.Http;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using NotEnoughLogs.Sinks;

namespace Refresh.Common;

/// <summary>
/// A common implementation of a Refresh server.
/// </summary>
public abstract class RefreshServer : IDisposable
{
    public BunkumHttpServer Server { get; protected init; }
    public Logger Logger => this.Server.Logger;

    protected RefreshServer(BunkumHttpListener? listener = null)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        (LoggerConfiguration logConfig, List<ILoggerSink>? sinks) = this.GetLoggerConfiguration();
        
        this.Server = new BunkumHttpServer(logConfig, sinks);
        if(listener != null) this.Server.UseListener(listener);
        
        this.Logger.LogDebug(BunkumCategory.Startup, $"Initializing {nameof(RefreshServer)} of type {this.GetType().Name}");
        
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    // must be called when extending RefreshServer
    protected void SetupInitializer(Action preServerInitializer)
    {
        this.Server.Initialize = _ =>
        {
            this.SetupConfiguration();
            preServerInitializer();
            this.Initialize();
        };
    }
    
    /// <summary>
    /// Starts the server. Does not block for you - to do so you must call <c>Task.Delay(-1)</c> 
    /// </summary>
    public virtual void Start()
    {
        this.Server.Start();
    }

    /// <summary>
    /// Triggers the server to stop accepting new connections and dispose Bunkum's internal objects.
    /// </summary>
    public virtual void Stop()
    {
        this.Server.Stop();
    }
    
    protected virtual void Initialize()
    {
        this.SetupServices();
        this.SetupMiddlewares();
        
        this.Server.DiscoverEndpointsFromAssembly(Assembly.GetCallingAssembly());
    }

    protected abstract void SetupServices();
    protected abstract void SetupMiddlewares();
    protected abstract void SetupConfiguration();
    
    protected virtual (LoggerConfiguration logConfig, List<ILoggerSink>? sinks) GetLoggerConfiguration()
    {
        LoggerConfiguration logConfig = new()
        {
            Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
            MaxLevel = LogLevel.Debug,
#else
            MaxLevel = LogLevel.Info,
#endif
        };
        
        return (logConfig, null);
    }

    public virtual void Dispose()
    {
        this.Logger.Dispose();
        GC.SuppressFinalize(this);
    }
}