using AttribDoc.Attributes;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Services;

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
    
    [ApiV3Endpoint("instance"), Authentication(false)]
    [DocSummary("Retrieves various information and metadata about the Refresh instance.")]
    public ApiResponse<ApiInstanceResponse> GetInstanceInformation(RequestContext context, GameServerConfig config, GameDatabaseContext database) 
        => new ApiInstanceResponse
        {
            InstanceName = config.InstanceName,
            InstanceDescription = config.InstanceDescription,
            RegistrationEnabled = config.RegistrationEnabled,
            SoftwareName = "Refresh",
            SoftwareVersion = "0.0.0", // TODO: Implement software version
            MaximumAssetSafetyLevel = config.MaximumAssetSafetyLevel,
            Announcements = ApiGameAnnouncementResponse.FromOldList(database.GetAnnouncements()),

#if DEBUG
            SoftwareType = "Debug",
#else
            SoftwareType = "Release",
#endif
        };
}