using System.Net.Http.Headers;
using NotEnoughLogs;
using Refresh.Common.Constants;
using Refresh.PresenceServer.Server;
using Refresh.PresenceServer.Server.Config;

namespace Refresh.PresenceServer.ApiClient;

public class RefreshPresenceApiClient : IDisposable
{
    private readonly PresenceServerConfig _config;
    private readonly Logger _logger;
    private readonly HttpClient _client;

    public RefreshPresenceApiClient(PresenceServerConfig config, Logger logger)
    {
        this._config = config;
        this._logger = logger;

        this._client = new HttpClient();
        this._client.BaseAddress = new Uri(this._config.GameServerUrl);
        this._client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(this._config.SharedSecret);
    }

    public async Task TestRefreshServer()
    {
        try
        {
            HttpResponseMessage result = await this._client.PostAsync(EndpointRoutes.PresenceBaseRoute + "test", new ByteArrayContent([]));

            switch (result.StatusCode)
            {
                case NotFound:
                    throw new Exception("Unable to access presence endpoint! Are you sure Refresh is up to date?");
                case NotImplemented:
                    throw new Exception("Presence integration is not enabled in Refresh!");
                case Unauthorized:
                    throw new Exception("Presence shared secret does not match!");
                default:
                    throw new Exception($"Unexpected status code {result.StatusCode} when accessing presence API!");
                case OK:
                    return;
            }
        }
        catch(Exception)
        {
            this._logger.LogWarning(PresenceCategory.Startup, "Unable to access Refresh game server! Are you sure it is configured correctly?");   
        }
    }

    public async Task<bool> InformConnection(string token)
    {
        try
        {
            HttpResponseMessage result = await this._client.PostAsync(EndpointRoutes.PresenceBaseRoute + "informConnection",
                new StringContent(token));

            switch (result.StatusCode)
            {
                case OK:
                    return true;
                case NotFound:
                    this._logger.LogWarning(PresenceCategory.Connections, $"Unknown user ({token}) tried to connect to presence server, disconnecting.");
                    return false;
                default:
                    throw new Exception($"Unexpected status code {result.StatusCode} when accessing presence API!");
            }
        }
        catch (Exception)
        {
            this._logger.LogWarning(PresenceCategory.Connections, "Unable to connect to Refresh to inform about a connection.");
            return false;
        }
    }
    
    public async Task InformDisconnection(string token)
    {
        try
        {
            HttpResponseMessage result = await this._client.PostAsync(EndpointRoutes.PresenceBaseRoute + "informDisconnection",
                new StringContent(token));

            switch (result.StatusCode)
            {
                case OK:
                // this can happen if a `goodbye` request comes to the gameserver before our request makes it to Refresh, or maybe they get banned. Something of the sort.
                case NotFound: 
                    return;
                default:
                    throw new Exception($"Unexpected status code {result.StatusCode} when accessing presence API!");
            }
        }
        catch (Exception)
        {
            this._logger.LogWarning(PresenceCategory.Connections, "Unable to connect to Refresh to inform about a disconnection.");
        }
    }

    public void Dispose()
    {
        this._client.Dispose();
    }
}