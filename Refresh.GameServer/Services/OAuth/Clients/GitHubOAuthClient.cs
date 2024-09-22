using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using NotEnoughLogs;
using Refresh.Common.Extensions;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.OAuth;
using Refresh.GameServer.Types.OAuth.Discord.Api;
using Refresh.GameServer.Types.OAuth.GitHub;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services.OAuth.Clients;

public class GitHubOAuthClient : OAuthClient
{
    public GitHubOAuthClient(Logger logger, IntegrationConfig integrationConfig) : base(logger)
    {
        this.ClientId = integrationConfig.GitHubOAuthClientId;
        this.ClientSecret = integrationConfig.GitHubOAuthClientSecret;
        this.RedirectUri = integrationConfig.GitHubOAuthRedirectUrl;
    }

    public override void Initialize()
    {
        base.Initialize();
        
        // This isn't strictly required, but will prevent future breakages,
        // since GitHub commits to supporting older API versions when they make breaking changes
        this.Client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
    }

    public override OAuthProvider Provider => OAuthProvider.GitHub;
    protected override string TokenEndpoint => "https://github.com/login/oauth/access_token";
    protected override string TokenRevocationEndpoint => $"https://api.github.com/applications/{this.ClientId}/grant";
    public override bool TokenRevocationSupported => true;
    
    protected override string ClientId { get; }
    protected override string ClientSecret { get; }
    protected override string RedirectUri { get; }
    
    public override string GetOAuthAuthorizationUrl(string state)
    {
        NameValueCollection queryParams = new()
        {
            ["client_id"] = this.ClientId,
            ["response_type"] = "code",
            ["state"] = state,
            ["redirect_uri"] = this.RedirectUri,
            ["scope"] = "read:user",
        };

        return $"https://github.com/login/oauth/authorize{queryParams.ToQueryString()}";
    }

    private string GetAccessTokenBody(OAuthTokenRelation token) =>
        $"{{\"access_token\":\"{HttpUtility.JavaScriptStringEncode(token.AccessToken)}\"}}";

    // GitHub doesn't implement RFC 7009, so we have to write special logic for it :/
    // See https://docs.github.com/en/rest/apps/oauth-applications?apiVersion=2022-11-28#delete-an-app-authorization
    public override void RevokeToken(GameDatabaseContext database, OAuthTokenRelation token)
    {
        HttpRequestMessage message = new(HttpMethod.Delete, this.TokenRevocationEndpoint);

        // this particular endpoint is special, we cant revoke a token by authenticating as the token, only by authenticating as the OAuth app
        message.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{this.ClientId}:{this.ClientSecret}")));
        
        message.Headers.Accept.Clear();
        message.Headers.Accept.ParseAdd("application/vnd.github+json");
        
        message.Content = new StringContent(this.GetAccessTokenBody(token)); 
        
        HttpResponseMessage response = this.Client.Send(message);

        // This is the success response
        if (response.StatusCode == NoContent)
            return;

        // This is sent when the token is already invalid
        if (response.StatusCode == NotFound)
            return;

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Got unexpected error status code {response.StatusCode} when revoking token!");
    }

    public GitHubApiUserResponse? GetUserInformation(GameDatabaseContext database, IDateTimeProvider timeProvider, GameUser user)
        => this.GetUserInformation(database, timeProvider, database.GetOAuthTokenFromUser(user, OAuthProvider.GitHub));
    
    public GitHubApiUserResponse? GetUserInformation(GameDatabaseContext database, IDateTimeProvider timeProvider, OAuthTokenRelation? token)
    {
        if (token == null)
            return null;

        HttpResponseMessage? response = this.MakeRequest(token, () => this.CreateRequestMessage(token, HttpMethod.Get, "https://api.github.com/user"), database, timeProvider);

        if (response == null)
            return null;
        
        if (!response.IsSuccessStatusCode) 
            throw new Exception($"Request returned status code {response.StatusCode}");

        if ((response.Content.Headers.ContentLength ?? 0) == 0)
            throw new Exception("Request returned no response");

        return response.Content.ReadAsJson<GitHubApiUserResponse>()!;
    }
}