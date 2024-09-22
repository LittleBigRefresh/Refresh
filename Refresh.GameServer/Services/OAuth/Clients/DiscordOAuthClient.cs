using System.Collections.Specialized;
using System.Net.Http.Headers;
using NotEnoughLogs;
using Refresh.Common.Extensions;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.OAuth;
using Refresh.GameServer.Types.OAuth.Discord.Api;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services.OAuth.Clients;

public class DiscordOAuthClient : OAuthClient
{
    public DiscordOAuthClient(Logger logger, IntegrationConfig integrationConfig) : base(logger)
    {
        this.ClientId = integrationConfig.DiscordOAuthClientId;
        this.ClientSecret = integrationConfig.DiscordOAuthClientSecret;
        this.RedirectUri = integrationConfig.DiscordOAuthRedirectUrl;
    }

    public override OAuthProvider Provider => OAuthProvider.Discord;

    protected override string TokenEndpoint => "https://discord.com/api/oauth2/token";
    protected override string TokenRevocationEndpoint => "https://discord.com/api/oauth2/token/revoke";
    public override bool TokenRevocationSupported => true;
    protected override string ClientId { get; }
    protected override string ClientSecret { get; }
    protected override string RedirectUri { get; }
    
    /// <inheritdoc />
    public override string GetOAuthAuthorizationUrl(string state)
    {
        NameValueCollection queryParams = new()
        {
            ["client_id"] = this.ClientId,
            ["response_type"] = "code",
            ["state"] = state,
            ["redirect_uri"] = this.RedirectUri,
            ["scope"] = "identify",
        };

        return $"https://discord.com/oauth2/authorize{queryParams.ToQueryString()}";
    }

    /// <summary>
    /// Gets information about a user's linked discord account
    /// </summary>
    /// <param name="database">The database used to access the user's token</param>
    /// <param name="timeProvider">The time provider for the current request</param>
    /// <param name="user">The user to get information on</param>
    /// <returns>The acquired user information, or null if the token is missing/expired</returns>
    public DiscordApiUserResponse? GetUserInformation(GameDatabaseContext database, IDateTimeProvider timeProvider, GameUser user)
        => this.GetUserInformation(database, timeProvider, database.GetOAuthTokenFromUser(user, OAuthProvider.Discord));

    private DiscordApiUserResponse? GetUserInformation(GameDatabaseContext database, IDateTimeProvider timeProvider, OAuthTokenRelation? token)
    {
        if (token == null)
            return null;

        HttpResponseMessage? response = this.MakeRequest(token, () => this.CreateRequestMessage(token, HttpMethod.Get, "https://discord.com/api/users/@me"), database, timeProvider);

        if (response == null)
            return null;
        
        if (!response.IsSuccessStatusCode) 
            throw new Exception($"Request returned status code {response.StatusCode}");

        if ((response.Content.Headers.ContentLength ?? 0) == 0)
            throw new Exception("Request returned no response");

        return response.Content.ReadAsJson<DiscordApiUserResponse>()!;
    }
}