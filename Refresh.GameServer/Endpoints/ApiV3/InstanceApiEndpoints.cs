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
    public ApiResponse<ApiStatisticsResponse> GetStatistics(RequestContext context, GameDatabaseContext database, MatchService match) 
        => new ApiStatisticsResponse
        {
            TotalLevels = database.GetTotalLevelCount(),
            TotalUsers = database.GetTotalUserCount(),
            TotalPhotos = database.GetTotalPhotoCount(),
            CurrentRoomCount = match.Rooms.Count(),
            CurrentIngamePlayersCount = match.TotalPlayers,
        };
    
    [ApiV3Endpoint("instance"), Authentication(false), AllowDuringMaintenance]
    [DocSummary("Retrieves various information and metadata about the Refresh instance.")]
    public ApiResponse<ApiInstanceResponse> GetInstanceInformation(RequestContext context,
        GameServerConfig gameConfig,
        RichPresenceConfig richConfig,
        GameDatabaseContext database) 
        => new ApiInstanceResponse
        {
            InstanceName = gameConfig.InstanceName,
            InstanceDescription = gameConfig.InstanceDescription,
            RegistrationEnabled = gameConfig.RegistrationEnabled,
            SoftwareName = "Refresh",
            SoftwareVersion = "0.0.0", // TODO: Implement software version
            MaximumAssetSafetyLevel = gameConfig.MaximumAssetSafetyLevel,
            Announcements = ApiGameAnnouncementResponse.FromOldList(database.GetAnnouncements()),
            MaintenanceModeEnabled = gameConfig.MaintenanceMode,
            RichPresenceConfiguration = ApiRichPresenceConfigurationResponse.FromOld(RichPresenceConfiguration.Create(gameConfig, richConfig))!,

#if DEBUG
            SoftwareType = "Debug",
#else
            SoftwareType = "Release",
#endif
        };
}