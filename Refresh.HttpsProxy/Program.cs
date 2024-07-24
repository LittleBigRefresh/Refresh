// See https://aka.ms/new-console-template for more information

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

ProxyConfig config = Config.LoadFromJsonFile<ProxyConfig>("proxy.json", httpsServer.Logger);

httpsServer.Initialize = s =>
{
    s.AddMiddleware(new ProxyMiddleware(config));
};

httpServer.Initialize = s =>
{
    s.AddMiddleware(new ProxyMiddleware(config));
};

// Start the server in multi-threaded mode, and let Bunkum manage the rest.
httpsServer.Start();
httpServer.Start();
await Task.Delay(-1);