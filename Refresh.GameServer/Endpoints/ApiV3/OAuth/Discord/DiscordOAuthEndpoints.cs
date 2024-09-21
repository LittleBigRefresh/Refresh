using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth.Discord;
using Refresh.GameServer.Services.OAuth;
using Refresh.GameServer.Services.OAuth.Clients;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.OAuth;
using Refresh.GameServer.Types.OAuth.Discord.Api;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.OAuth.Discord;

public class DiscordOAuthEndpoints : EndpointGroup
{
    [ApiV3Endpoint("oauth/discord/currentUserInformation")]
    public ApiResponse<ApiDiscordUserResponse> CurrentUserInformation(
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
        
        return ApiDiscordUserResponse.FromOld(userInformation, dataContext);
    }
}