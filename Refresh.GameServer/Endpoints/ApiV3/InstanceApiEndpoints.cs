using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.RichPresence;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class InstanceApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("statistics"), Authentication(false)]
    [DocSummary("Retrieves various statistics about the Refresh instance.")]
    public ApiResponse<ApiStatisticsResponse> GetStatistics(RequestContext context, GameDatabaseContext database,
        MatchService match, GameServerConfig config, DataContext dataContext)
    {
        ApiRequestStatisticsResponse requestStatistics = ApiRequestStatisticsResponse.FromOld(database.GetRequestStatistics(), dataContext)!;

        RoomStatistics statistics = match.RoomAccessor.GetStatistics();
        
        return new ApiStatisticsResponse
        {
            TotalLevels = database.GetTotalLevelCount(),
            ModdedLevels = database.GetModdedLevelCount(),
            TotalUsers = database.GetTotalUserCount(),
            ActiveUsers = database.GetActiveUserCount(),
            TotalPhotos = database.GetTotalPhotoCount(),
            TotalEvents = database.GetTotalEventCount(),
            CurrentRoomCount = statistics.RoomCount,
            CurrentIngamePlayersCount = statistics.PlayerCount,
            RequestStatistics = requestStatistics,
        };
    }

    [ApiV3Endpoint("instance"), Authentication(false), AllowDuringMaintenance]
    [ClientCacheResponse(3600)] // One hour
    [DocSummary("Retrieves various information and metadata about the Refresh instance.")]
    public ApiResponse<ApiInstanceResponse> GetInstanceInformation(RequestContext context,
        GameServerConfig gameConfig,
        RichPresenceConfig richConfig,
        IntegrationConfig integrationConfig,
        ContactInfoConfig contactInfoConfig,
        GameDatabaseContext database,
        DataContext dataContext) 
        => new ApiInstanceResponse
        {
            InstanceName = gameConfig.InstanceName,
            InstanceDescription = gameConfig.InstanceDescription,
            RegistrationEnabled = gameConfig.RegistrationEnabled,
            SoftwareName = "Refresh",
            SoftwareVersion = VersionInformation.Version,
            SoftwareSourceUrl = "https://github.com/LittleBigRefresh/Refresh",
            SoftwareLicenseName = "AGPL-3.0",
            SoftwareLicenseUrl = "https://www.gnu.org/licenses/agpl-3.0.txt",
            BlockedAssetFlags = gameConfig.BlockedAssetFlags,
            BlockedAssetFlagsForTrustedUsers = gameConfig.BlockedAssetFlagsForTrustedUsers,
            Announcements = ApiGameAnnouncementResponse.FromOldList(database.GetAnnouncements(), dataContext),
            MaintenanceModeEnabled = gameConfig.MaintenanceMode,
            RichPresenceConfiguration = ApiRichPresenceConfigurationResponse.FromOld(RichPresenceConfiguration.Create(
                gameConfig,
                richConfig), dataContext)!,
            GrafanaDashboardUrl = integrationConfig.GrafanaDashboardUrl,
            WebsiteLogoUrl = integrationConfig.WebsiteLogoUrl,
            
            ContactInfo = new ApiContactInfoResponse
            {
                AdminName = contactInfoConfig.AdminName,
                EmailAddress = contactInfoConfig.EmailAddress,
                DiscordServerInvite = contactInfoConfig.DiscordServerInvite,
                AdminDiscordUsername = contactInfoConfig.AdminDiscordUsername,
            },
            
            ActiveContest = ApiContestResponse.FromOld(database.GetNewestActiveContest(), dataContext),
#if DEBUG
            SoftwareType = "Debug",
#else
            SoftwareType = "Release",
#endif
        };
}