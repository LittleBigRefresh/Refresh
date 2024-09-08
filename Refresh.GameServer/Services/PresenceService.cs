using System.Net.Http.Headers;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public class PresenceService : EndpointService
{
    private readonly IntegrationConfig _config;

    private readonly HttpClient _client;
    
    public PresenceService(Logger logger, IntegrationConfig config) : base(logger)
    {
        this._config = config;

        this._client = new HttpClient();
        
        this._client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(config.PresenceSharedSecret);
        this._client.BaseAddress = new Uri(config.PresenceBaseUrl);
    }

    /// <summary>
    /// Tries to inform the presence server to tell a user to play a level
    /// </summary>
    /// <param name="user">The user to inform</param>
    /// <param name="levelId">The level to play</param>
    /// <returns>Whether or not the server was informed correctly</returns>
    public bool PlayLevel(GameUser user, int levelId)
    {
        // Block requests if presence isn't enabled or the user is not authenticated with the presence server
        if (!this._config.PresenceEnabled || user.PresenceServerAuthToken == null)
            return false;
        
        this.Logger.LogInfo(RefreshContext.Presence, $"Sending presence request for level ID {levelId} to {user}");
        
        HttpResponseMessage result = this._client.PostAsync($"/api/playLevel/{levelId}", new StringContent(user.PresenceServerAuthToken)).Result;

        if (result.IsSuccessStatusCode)
            return true;

        if(result.StatusCode == NotFound)
            return false;

        this.Logger.LogWarning(RefreshContext.Presence, "Received status code {0} {1} while trying to communicate with the presence server.", (int)result.StatusCode, result.StatusCode);
        
        return false;
    }
}