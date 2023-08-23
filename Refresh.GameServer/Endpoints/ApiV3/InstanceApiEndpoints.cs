using AttribDoc.Attributes;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.RichPresence;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class InstanceApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("statistics"), Authentication(false)]
    [DocSummary("Retrieves various statistics about the Refresh instance.")]
    public ApiResponse<ApiStatisticsResponse> GetStatistics(RequestContext context, GameDatabaseContext database, MatchService match, GameServerConfig config)
    {
        ApiRequestStatisticsResponse requestStatistics;
        if (!config.TrackRequestStatistics)
        {
            requestStatistics = new ApiRequestStatisticsResponse
            {
                TotalRequests = -1,
                ApiRequests = -1,
                LegacyApiRequests = -1,
                GameRequests = -1,
            };
        }
        else
        {
            requestStatistics = ApiRequestStatisticsResponse.FromOld(database.GetRequestStatistics())!;
        }
        
        return new ApiStatisticsResponse
        {
            TotalLevels = database.GetTotalLevelCount(),
            TotalUsers = database.GetTotalUserCount(),
            TotalPhotos = database.GetTotalPhotoCount(),
            TotalEvents = database.GetTotalEventCount(),
            CurrentRoomCount = match.Rooms.Count(),
            CurrentIngamePlayersCount = match.TotalPlayers,
            RequestStatistics = requestStatistics,
        };
    }

    [ApiV3Endpoint("instance"), Authentication(false), AllowDuringMaintenance]
    [DocSummary("Retrieves various information and metadata about the Refresh instance.")]
    public ApiResponse<ApiInstanceResponse> GetInstanceInformation(RequestContext context,
        GameServerConfig gameConfig,
        RichPresenceConfig richConfig,
        IntegrationConfig integrationConfig,
        GameDatabaseContext database) 
        => new ApiInstanceResponse
        {
            InstanceName = gameConfig.InstanceName,
            InstanceDescription = gameConfig.InstanceDescription,
            RegistrationEnabled = gameConfig.RegistrationEnabled,
            SoftwareName = "Refresh",
            SoftwareVersion = VersionInformation.Version,
            MaximumAssetSafetyLevel = gameConfig.MaximumAssetSafetyLevel,
            Announcements = ApiGameAnnouncementResponse.FromOldList(database.GetAnnouncements()),
            MaintenanceModeEnabled = gameConfig.MaintenanceMode,
            RichPresenceConfiguration = ApiRichPresenceConfigurationResponse.FromOld(RichPresenceConfiguration.Create(gameConfig, richConfig))!,
            GrafanaDashboardUrl = integrationConfig.GrafanaDashboardUrl,

#if DEBUG
            SoftwareType = "Debug",
#else
            SoftwareType = "Release",
#endif
        };
}