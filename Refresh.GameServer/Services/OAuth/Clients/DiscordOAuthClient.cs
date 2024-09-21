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
    private readonly IntegrationConfig _integrationConfig;
    
    public DiscordOAuthClient(Logger logger, IntegrationConfig integrationConfig) : base(logger)
    {
        this._integrationConfig = integrationConfig;
    }

    public override OAuthProvider Provider => OAuthProvider.Discord;

    protected override Uri HttpBaseAddress => new("https://discord.com/api/");
    protected override string TokenEndpoint => "oauth2/token";
    protected override string TokenRevocationEndpoint => "oauth2/token/revoke";
    public override bool TokenRevocationSupported => true;
    protected override string ClientId => this._integrationConfig.DiscordOAuthClientId;
    protected override string ClientSecret => this._integrationConfig.DiscordOAuthClientSecret;
    protected override string RedirectUri => this._integrationConfig.DiscordOAuthRedirectUrl;
    
    /// <inheritdoc />
    public override string GetOAuthAuthorizationUrl(string state)
    {
        NameValueCollection queryParams = new()
        {
            ["client_id"] = this._integrationConfig.DiscordOAuthClientId,
            ["response_type"] = "code",
            ["state"] = state,
            ["redirect_uri"] = this._integrationConfig.DiscordOAuthRedirectUrl,
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

    private DiscordApiUserResponse? GetUserInformation(GameDatabaseContext database, IDateTimeProvider timeProvider, OAuthTokenRelation? token, bool justRefreshed = false)
    {
        if (token == null)
            return null;

        // TODO: this token refresh logic needs to be made generic so that it doesn't have to be copied to any and all API request methods for all clients
        
        // If we have passed the revocation time, and refreshing the token has failed, then revoke the token and bail out.
        if (timeProvider.Now >= token.AccessTokenRevocationTime && !this.RefreshToken(database, token, timeProvider))
        {
            database.RevokeOAuthToken(token);
            return null;
        }
        
        HttpRequestMessage message = new(HttpMethod.Get, "users/@me");
        message.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + token.AccessToken!);

        HttpResponseMessage result = this.Client.Send(message);

        if (result.StatusCode == Unauthorized)
        {
            // If we are unauthorized and have already tried to refresh the token with no success, revoke the token and bail out.
            if (justRefreshed || !this.RefreshToken(database, token, timeProvider))
            {
                database.RevokeOAuthToken(token);
                return null;
            }
            
            // Try again, after refreshing the token
            return this.GetUserInformation(database, timeProvider, token, true);
        }
        
        if (!result.IsSuccessStatusCode) 
            throw new Exception($"Request returned status code {result.StatusCode}");

        if ((result.Content.Headers.ContentLength ?? 0) == 0)
            throw new Exception("Request returned no response");

        return result.Content.ReadAsJson<DiscordApiUserResponse>()!;
    }
}