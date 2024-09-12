using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth2.Discord;
using Refresh.GameServer.Services;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.OAuth2.Discord;
using Refresh.GameServer.Types.OAuth2.Discord.Api;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.OAuth2.Discord;

public class DiscordOAuth2Endpoints : EndpointGroup
{
    [ApiV3Endpoint("oauth2/discord/beginAuthentication")]
    public ApiResponse<ApiDiscordOAuth2BeginAuthenticationResponse> BeginAuthentication(
        RequestContext context, 
        GameUser user, 
        GameDatabaseContext database, 
        DiscordOAuth2Service oAuth2Service,
        IDateTimeProvider timeProvider)
    {
        string state = database.CreateDiscordOAuth2Request(user, timeProvider);

        return new ApiDiscordOAuth2BeginAuthenticationResponse
        {
            AuthorizationUrl = oAuth2Service.ConstructOAuth2AuthorizationUrl(state),
        };
    }
    
    [ApiV3Endpoint("oauth2/discord/authenticate"), Authentication(false)]
    public Response Authenticate(
        RequestContext context,
        DiscordOAuth2Service oAuth2Service,
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

        // If the request doesn't exist, then it probably expired or someone meddled with the state data
        if (!database.DiscordOAuth2RequestExists(state))
            return BadRequest;
        
        DiscordApiOAuth2AccessTokenResponse response = oAuth2Service.AuthenticateToken(authCode);

        // Finish authentication with discord
        database.AuthenticateDiscordOAuth2Request(state, response, timeProvider);
        
        // TODO: add flag marking we came from here or something so that the frontend can say "You have been authenticated with discord!"
        context.ResponseHeaders["Location"] = config.WebExternalUrl;
        
        return Redirect;
    }

    [ApiV3Endpoint("oauth2/discord/currentUserInformation")]
    public ApiResponse<ApiDiscordOAuth2UserResponse> CurrentUserInformation(
        RequestContext context,
        GameDatabaseContext database,
        DiscordOAuth2Service oAuth2Service,
        GameUser user,
        IDateTimeProvider timeProvider,
        DataContext dataContext)
    {
        DiscordOAuthTokenRelation? token = database.GetDiscordOAuth2TokenFromUser(user);
        if (token == null)
            return ApiNotFoundError.DiscordOAuth2TokenMissingError;
        
        return ApiDiscordOAuth2UserResponse.FromOld(oAuth2Service.GetUserInformation(database, token, timeProvider), dataContext);
    }
}