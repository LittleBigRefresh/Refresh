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

        UriBuilder baseAddress = new(this._config.GameServerUrl)
        {
            Path = EndpointRoutes.PresenceBaseRoute,
        };

        this._client = new HttpClient();
        this._client.BaseAddress = baseAddress.Uri;
        this._client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(this._config.SharedSecret);
    }

    public async Task TestRefreshServer()
    {
        try
        {
            HttpResponseMessage result = await this._client.PostAsync("test", new ByteArrayContent([]));

            switch (result.StatusCode)
            {
                case NotFound:
                    throw new Exception("The presence endpoint wasn't found. This likely means Refresh.GameServer is out of date.");
                case NotImplemented:
                    throw new Exception("Presence integration is disabled in Refresh.GameServer");
                case Unauthorized:
                    throw new Exception("Our shared secret does not match the server's shared secret. Please check the config files in both Refresh.PresenceServer and Refresh.GameServer.");
                default:
                    throw new Exception($"Unexpected status code {(int)result.StatusCode} {result.StatusCode} when accessing presence API");
                case OK:
                    return;
            }
        }
        catch(Exception e)
        {
            this._logger.LogError(PresenceCategory.Startup, "Unable to access Refresh gameserver: {0}", e);   
        }
    }

    public async Task<bool> InformConnection(string token)
    {
        try
        {
            HttpResponseMessage result = await this._client.PostAsync("informConnection", new StringContent(token));

            switch (result.StatusCode)
            {
                case OK:
                    return true;
                case NotFound:
                    this._logger.LogWarning(PresenceCategory.Connections, $"Unknown user ({token}) tried to connect to presence server, disconnecting.");
                    return false;
                default:
                    throw new Exception($"Unexpected status code {(int)result.StatusCode} {result.StatusCode} when accessing presence API");
            }
        }
        catch (Exception)
        {
            this._logger.LogError(PresenceCategory.Connections, "Unable to connect to Refresh to inform about a connection.");
            return false;
        }
    }
    
    public async Task InformDisconnection(string token)
    {
        try
        {
            HttpResponseMessage result = await this._client.PostAsync("informDisconnection", new StringContent(token));

            switch (result.StatusCode)
            {
                case OK:
                case NotFound: 
                    return;
                default:
                    throw new Exception($"Unexpected status code {result.StatusCode} when accessing presence API!");
            }
        }
        catch (Exception)
        {
            this._logger.LogError(PresenceCategory.Connections, "Unable to connect to Refresh to inform about a disconnection.");
        }
    }

    public void Dispose()
    {
        this._client.Dispose();
    }
}