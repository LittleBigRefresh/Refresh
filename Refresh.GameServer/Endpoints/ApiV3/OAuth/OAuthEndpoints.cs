using System.Diagnostics;
using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth;
using Refresh.GameServer.Services.OAuth;
using Refresh.GameServer.Services.OAuth.Clients;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.OAuth;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.OAuth;

public class OAuthEndpoints : EndpointGroup
{
    [ApiV3Endpoint("oauth/{providerStr}/beginAuthentication")]
    [DocSummary("Begins the OAuth authentication process with the specified provider.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.OAuthProviderMissingErrorWhen)]
    [DocError(typeof(ApiNotSupportedError), ApiNotFoundError.OAuthProviderMissingErrorWhen)]
    public ApiResponse<ApiOAuthBeginAuthenticationResponse> BeginAuthentication(
        RequestContext context, 
        GameUser user, 
        GameDatabaseContext database, 
        OAuthService oAuthService,
        IDateTimeProvider timeProvider,
        [DocSummary("The OAuth provider to use for the authentication process")] string providerStr)
    {
        if (!Enum.TryParse(providerStr, true, out OAuthProvider provider))
            return ApiNotFoundError.OAuthProviderMissingError;
        
        if (!oAuthService.GetOAuthClient(provider, out OAuthClient? client))
            return ApiNotSupportedError.OAuthProviderTokenRevocationUnsupportedError;

        // Create a new OAuth request
        string state = database.CreateOAuthRequest(user, timeProvider, provider);

        return new ApiOAuthBeginAuthenticationResponse
        {
            AuthorizationUrl = client.GetOAuthAuthorizationUrl(state),
        };
    }
    
    [ApiV3Endpoint("oauth/authenticate"), Authentication(false)]
    [DocSummary("Finishes OAuth authentication and saves the token to the database. " +
                "This isn't meant to be called normally, and is intended as a redirect target of an OAuth authorization request")]
    public Response Authenticate(
        RequestContext context,
        OAuthService oAuthService,
        GameServerConfig config,
        GameDatabaseContext database,
        IDateTimeProvider timeProvider)
    {
        string? authCode = context.QueryString["code"];
        if (authCode == null)
            return BadRequest;

        string? state = context.QueryString["state"];
        if (state == null)
            return BadRequest;

        OAuthProvider? provider = database.OAuthGetProviderForRequest(state);
        // If the request doesn't exist, then it probably expired or the state data is invalid somehow
        if (provider == null)
            return BadRequest;

        OAuthClient? client = oAuthService.GetOAuthClient<DiscordOAuthClient>(provider.Value);
        Debug.Assert(client != null);
        
        OAuth2AccessTokenResponse response = client.AcquireTokenFromAuthorizationCode(authCode);

        // Save the OAuth token to the database
        GameUser user = database.SaveOAuthToken(state, response, timeProvider);
        
        context.ResponseHeaders["Location"] = config.WebExternalUrl;
        
        database.AddNotification("Account Linking Success", $"Your account has been successfully linked to {provider}!", user);
        
        return Redirect;
    }

    [ApiV3Endpoint("oauth/{providerStr}/revokeToken", HttpMethods.Post)]
    [DocSummary("Revokes the current user's OAuth token for the specified provider")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.OAuthProviderMissingErrorWhen)]
    [DocError(typeof(ApiNotSupportedError), ApiNotFoundError.OAuthProviderMissingErrorWhen)]
    public ApiResponse<ApiEmptyResponse> RevokeToken(RequestContext context,
        GameDatabaseContext database,
        OAuthService oAuthService,
        GameUser user,
        [DocSummary("The OAuth provider which provided the token to revoke")] string providerStr)
    {
        if (!Enum.TryParse(providerStr, true, out OAuthProvider provider))
            return ApiNotFoundError.OAuthProviderMissingError;
        
        if (!oAuthService.GetOAuthClient(provider, out OAuthClient? client))
            return ApiNotSupportedError.OAuthProviderTokenRevocationUnsupportedError;

        if (!client.TokenRevocationSupported)
            return ApiNotSupportedError.OAuthProviderTokenRevocationUnsupportedError;

        OAuthTokenRelation? token = database.GetOAuthTokenFromUser(user, provider);
        if (token == null)
            return ApiNotFoundError.OAuthTokenMissingError;
        
        client.RevokeToken(database, token);
        
        return new ApiOkResponse();
    }
}