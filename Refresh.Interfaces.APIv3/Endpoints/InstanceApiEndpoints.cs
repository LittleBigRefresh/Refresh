using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.Core;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Core.Types.Matching;
using Refresh.Core.Types.RichPresence;
using Refresh.Database;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

namespace Refresh.Interfaces.APIv3.Endpoints;

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
            WebsiteDefaultTheme = integrationConfig.WebsiteDefaultTheme,
            
            ContactInfo = new ApiContactInfoResponse
            {
                AdminName = contactInfoConfig.AdminName,
                EmailAddress = contactInfoConfig.EmailAddress,
                DiscordServerInvite = contactInfoConfig.DiscordServerInvite,
                AdminDiscordUsername = contactInfoConfig.AdminDiscordUsername,
            },
            
            ActiveContest = ApiContestResponse.FromOld(database.GetNewestActiveContest(), dataContext),
#if DEBUG && POSTGRES
            SoftwareType = "Debug (Postgres)",
#elif DEBUG
            SoftwareType = "Debug",
#elif POSTGRES
            SoftwareType = "Release (Postgres)",
#else
            SoftwareType = "Release",
#endif
        };
}