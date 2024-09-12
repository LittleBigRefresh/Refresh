using System.Collections.Specialized;
using System.Net.Http.Headers;
using Bunkum.Core.Configuration;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.Common.Extensions;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.OAuth2;
using Refresh.GameServer.Types.OAuth2.Discord;
using Refresh.GameServer.Types.OAuth2.Discord.Api;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public class DiscordOAuth2Service : EndpointService
{
    private readonly IntegrationConfig _integrationConfig;
    private readonly HttpClient _client;
    
    public DiscordOAuth2Service(Logger logger, IntegrationConfig integrationConfig) : base(logger)
    {
        this._integrationConfig = integrationConfig;

        this._client = new HttpClient();
        this._client.BaseAddress = new Uri("https://discord.com/api/");
    }

    public string ConstructOAuth2AuthorizationUrl(string state)
    {
        NameValueCollection queryParams = new()
        {
            ["client_id"] = this._integrationConfig.DiscordOAuth2ClientId,
            ["response_type"] = "code",
            ["state"] = state,
            ["redirect_uri"] = this._integrationConfig.DiscordOAuth2RedirectUrl,
            ["scope"] = "identify",
        };

        return $"https://discord.com/oauth2/authorize{queryParams.ToQueryString()}";
    }

    public DiscordApiOAuth2AccessTokenResponse AuthenticateToken(string authCode)
    {
        HttpResponseMessage result = this._client.PostAsync("oauth2/token", new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authCode),
            new KeyValuePair<string, string>("redirect_uri", this._integrationConfig.DiscordOAuth2RedirectUrl),
            new KeyValuePair<string, string>("client_id", this._integrationConfig.DiscordOAuth2ClientId),
            new KeyValuePair<string, string>("client_secret", this._integrationConfig.DiscordOAuth2ClientSecret),
        ])).Result;

        if (result.StatusCode == BadRequest)
        {
            OAuth2ErrorResponse errorResponse = result.Content.ReadAsJson<OAuth2ErrorResponse>()!;

            if (errorResponse.Error == "invalid_grant")
                throw new Exception("The auth code is invalid"); //TODO: give this a proper exception class which can be handled
            
            throw new Exception($"Unknown error {errorResponse.Error} when refreshing token! Description: {errorResponse.ErrorDescription}, URI: {errorResponse.ErrorUri}"); //TODO:
        }
        
        if (!result.IsSuccessStatusCode)
            throw new Exception(); //TODO
        
        DiscordApiOAuth2AccessTokenResponse response = result.Content.ReadAsJson<DiscordApiOAuth2AccessTokenResponse>()!;
        
        // Not sure why someone would want to forge a request to give us a bigger scope, but let's prevent it anyway
        if (response.Scope != "identify")
            throw new Exception(); //TODO
        
        return response;
    }

    public bool RefreshToken(GameDatabaseContext database, DiscordOAuthTokenRelation token, IDateTimeProvider timeProvider)
    {
        HttpResponseMessage result = this._client.PostAsync("oauth2/token", new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", token.RefreshToken),
            new KeyValuePair<string, string>("client_id", this._integrationConfig.DiscordOAuth2ClientId),
            new KeyValuePair<string, string>("client_secret", this._integrationConfig.DiscordOAuth2ClientSecret),
        ])).Result;

        // 400 Bad Request means error with refreshing token
        if (result.StatusCode == BadRequest)
        {
            OAuth2ErrorResponse errorResponse = result.Content.ReadAsJson<OAuth2ErrorResponse>()!;

            if (errorResponse.Error == "invalid_grant")
                return false;
            
            throw new Exception($"Unknown error {errorResponse.Error} when refreshing token! Description: {errorResponse.ErrorDescription}, URI: {errorResponse.ErrorUri}"); //TODO:
        }
        
        if (!result.IsSuccessStatusCode)
            throw new Exception(); //TODO
        
        DiscordApiOAuth2AccessTokenResponse response = result.Content.ReadAsJson<DiscordApiOAuth2AccessTokenResponse>()!;

        database.UpdateDiscordOAuth2Token(token, response, timeProvider);

        return true;
    }

    public DiscordApiUserResponse? GetUserInformation(DataContext dataContext, GameUser user)
        => this.GetUserInformation(dataContext.Database, dataContext.Database.GetDiscordOAuth2TokenFromUser(user),
            dataContext.TimeProvider);

    public DiscordApiUserResponse? GetUserInformation(GameDatabaseContext database, DiscordOAuthTokenRelation? token, IDateTimeProvider timeProvider)
    {
        if (token == null)
            return null;
        
        bool refreshed = false;
        while (true)
        {
            // If we are past the revocation time of the token, refresh it
            if (timeProvider.Now > token.AccessTokenRevocationTime) this.RefreshToken(database, token, timeProvider);

            HttpRequestMessage message = new(HttpMethod.Get, "users/@me");
            message.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + token.AccessToken!);

            HttpResponseMessage result = this._client.Send(message);
            
            if (result.StatusCode == Unauthorized)
            {
                // If we haven't refreshed and refreshing the token succeeds, then try to get the user info again
                if (!refreshed && this.RefreshToken(database, token, timeProvider))
                {
                    refreshed = true;

                    continue;
                }

                database.RemoveDiscordOAuth2Token(token);
                return null;
            }

            if (!result.IsSuccessStatusCode) throw new Exception(); //TODO

            DiscordApiUserResponse response = result.Content.ReadAsJson<DiscordApiUserResponse>()!;

            return response;
        }
    }
}