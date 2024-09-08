using Bunkum.Core.Configuration;
using Bunkum.Protocols.Http;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using Refresh.PresenceServer.ApiClient;
using Refresh.PresenceServer.ApiServer.Endpoints;
using Refresh.PresenceServer.ApiServer.Middlewares;
using Refresh.PresenceServer.Server.Config;

namespace Refresh.PresenceServer;

internal class Program
{
    public static Server.PresenceServer PresenceServer;
    
    public static async Task Main()
    {
        LoggerConfiguration loggerConfiguration = new()
        {
            Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
            MaxLevel = LogLevel.Trace,
#else
    MaxLevel = LogLevel.Info,
#endif
        };

        PresenceServerConfig config = null!;
        RefreshPresenceApiClient apiClient = null!;
        BunkumHttpServer apiServer = new(loggerConfiguration)
        {
            Initialize = server =>
            {
                config = Config.LoadFromJsonFile<PresenceServerConfig>("presenceServer.json", server.Logger);
                apiClient = new RefreshPresenceApiClient(config, server.Logger);

                server.DiscoverEndpointsFromAssembly(typeof(ApiEndpoints).Assembly);
                server.AddConfig(config);
                
                server.AddMiddleware(new SecretMiddleware(config));
            },
        };

        apiServer.Start();

        await apiClient.TestRefreshServer();

// Start both servers
        PresenceServer = new Server.PresenceServer(config, apiServer.Logger, apiClient);

        PresenceServer.Start();

        await Task.Delay(-1);
    }
}