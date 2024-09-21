using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth2.Discord;
using Refresh.GameServer.Services.OAuth2;
using Refresh.GameServer.Services.OAuth2.Clients;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.OAuth2;
using Refresh.GameServer.Types.OAuth2.Discord.Api;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.OAuth.Discord;

public class DiscordOAuthEndpoints : EndpointGroup
{
    [ApiV3Endpoint("oauth2/discord/currentUserInformation")]
    public ApiResponse<ApiDiscordOAuthUserResponse> CurrentUserInformation(
        RequestContext context,
        GameDatabaseContext database,
        OAuthService oAuthService,
        GameUser user,
        IDateTimeProvider timeProvider,
        DataContext dataContext)
    {
        if (!oAuthService.GetOAuthClient<DiscordOAuthClient>(OAuthProvider.Discord, out DiscordOAuthClient? client))
            return ApiNotFoundError.OAuthProviderMissingError;
        
        DiscordApiUserResponse? userInformation = client.GetUserInformation(database, timeProvider, user);
        
        if (userInformation == null)
            return ApiNotFoundError.DiscordOAuthTokenMissingError;
        
        return ApiDiscordOAuthUserResponse.FromOld(userInformation, dataContext);
    }
}