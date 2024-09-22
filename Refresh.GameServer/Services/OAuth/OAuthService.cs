using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Services.OAuth.Clients;
using Refresh.GameServer.Types.OAuth;

namespace Refresh.GameServer.Services.OAuth;

public class OAuthService : EndpointService
{
    private readonly IntegrationConfig _integrationConfig;
    
    private readonly Dictionary<OAuthProvider, OAuthClient> _clients;
    
    public OAuthService(Logger logger, IntegrationConfig integrationConfig) : base(logger)
    {
        this._integrationConfig = integrationConfig;
        this._clients = new Dictionary<OAuthProvider, OAuthClient>();
    }

    public override void Initialize()
    {
        base.Initialize();
        
        if (this._integrationConfig.DiscordOAuthEnabled)
            this._clients[OAuthProvider.Discord] = new DiscordOAuthClient(this.Logger, this._integrationConfig);
        if (this._integrationConfig.GitHubOAuthEnabled)
            this._clients[OAuthProvider.GitHub] = new GitHubOAuthClient(this.Logger, this._integrationConfig);
        
        // Initialize all the OAuth clients
        foreach ((OAuthProvider provider, OAuthClient? client) in this._clients)
        {
            this.Logger.LogInfo(RefreshContext.Startup, "Initializing {0} OAuth client", provider);
            client.Initialize();
        }
    }

    public bool GetOAuthClient(OAuthProvider provider, [MaybeNullWhen(false)] out OAuthClient client) => this._clients.TryGetValue(provider, out client);
    public bool GetOAuthClient<T>(OAuthProvider provider, [MaybeNullWhen(false)] out T client) where T : class
    {
        bool ret = this._clients.TryGetValue(provider, out OAuthClient? rawClient);

        if(rawClient != null) 
            Debug.Assert(rawClient.GetType().IsAssignableTo(typeof(T)), "Acquired client must be assignable to type parameter");
        
        client = rawClient as T;
        
        return ret;
    }

    public T? GetOAuthClient<T>(OAuthProvider provider) where T : class 
        => this._clients.GetValueOrDefault(provider) as T;
    
    public OAuthClient? GetOAuthClient(OAuthProvider provider)
        => this._clients.GetValueOrDefault(provider);
}