using Bunkum.Core;
using Bunkum.Core.Configuration;
using Bunkum.Protocols.Http;
using Bunkum.Protocols.Https;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using Refresh.HttpsProxy.Config;
using Refresh.HttpsProxy.Middlewares;

LoggerConfiguration loggerConfiguration = new()
{
    Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
    MaxLevel = LogLevel.Trace,
#else
    MaxLevel = LogLevel.Info,
#endif
};

// Initialize a Bunkum server for HTTPS
BunkumServer httpsServer = new BunkumHttpsServer(loggerConfiguration);

// Initialize a Bunkum server for HTTP
BunkumServer httpServer = new BunkumHttpServer(loggerConfiguration);

Action<BunkumServer> initialize = s =>
{
    ProxyConfig config = Config.LoadFromJsonFile<ProxyConfig>("proxy.json", s.Logger);
    s.AddMiddleware(new ProxyMiddleware(config));
};

httpsServer.Initialize = initialize;
httpServer.Initialize = initialize;

// Start the server in multi-threaded mode, and let Bunkum manage the rest.
httpsServer.Start();
httpServer.Start();
await Task.Delay(-1);