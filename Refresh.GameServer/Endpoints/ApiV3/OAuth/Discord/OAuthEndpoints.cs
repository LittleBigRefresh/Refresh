using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth2.Discord;
using Refresh.GameServer.Services.OAuth2;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.OAuth2;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.OAuth.Discord;

public class OAuthEndpoints : EndpointGroup
{
    [ApiV3Endpoint("oauth2/{provider}/beginAuthentication")]
    public ApiResponse<ApiOAuthBeginAuthenticationResponse> BeginAuthentication(
        RequestContext context, 
        GameUser user, 
        GameDatabaseContext database, 
        OAuthService oAuthService,
        IDateTimeProvider timeProvider,
        string providerStr)
    {
        if (!Enum.TryParse(providerStr, true, out OAuthProvider provider) || 
            !oAuthService.GetOAuthClient(provider, out OAuthClient? client))
            return ApiNotFoundError.OAuthProviderMissingError;
        
        // Create a new OAuth request
        string state = database.CreateOAuthRequest(user, timeProvider, provider);

        return new ApiOAuthBeginAuthenticationResponse
        {
            AuthorizationUrl = client.GetOAuth2AuthorizationUrl(state),
        };
    }
    
    [ApiV3Endpoint("oauth2/{provider}/authenticate"), Authentication(false)]
    public Response Authenticate(
        RequestContext context,
        OAuthService oAuthService,
        GameServerConfig config,
        GameDatabaseContext database,
        IDateTimeProvider timeProvider,
        string providerStr)
    {
        if (!Enum.TryParse(providerStr, true, out OAuthProvider provider) || 
            !oAuthService.GetOAuthClient(provider, out OAuthClient? client))
            return NotFound;
        
        string? authCode = context.QueryString["code"];
        if (authCode == null)
            return BadRequest;

        string? state = context.QueryString["state"];
        if (state == null)
            return BadRequest;

        // If the request doesn't exist, then it probably expired or the state data is invalid somehow
        if (!database.OAuthRequestExists(state, provider))
            return BadRequest;
        
        OAuth2AccessTokenResponse response = client.AcquireTokenFromAuthorizationCode(authCode);

        // Save the OAuth token to the database
        GameUser user = database.SaveOAuthToken(state, response, timeProvider);
        
        // TODO: add flag marking we came from here or something so that the frontend can say "You have been authenticated with auth provider X!"
        context.ResponseHeaders["Location"] = config.WebExternalUrl;
        
        database.AddNotification("Account Linking Success", $"Your account has been successfully linked to {provider}!", user);
        
        return Redirect;
    }
}