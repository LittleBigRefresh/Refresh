using System.Diagnostics;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Playlists;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Playlists;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGamePlaylistResponse : IApiResponse, IDataConvertableFrom<ApiGamePlaylistResponse, GamePlaylist>
{
    public required int PlaylistId { get; set; }
    public required ApiGameUserResponse? Publisher { get; set; }

    public required string Name { get; set; }
    public required string IconHash { get; set; }
    public required string Description { get; set; }
    public required ApiGameLocationResponse Location { get; set; }

    public required DateTimeOffset CreationDate { get; set; }
    public required DateTimeOffset UpdateDate { get; set; }

    public ApiGamePlaylistStatisticsResponse? Statistics { get; set; }
    public ApiGamePlaylistOwnRelationsResponse? OwnRelations { get; set; }

    public static ApiGamePlaylistResponse? FromOld(GamePlaylist? playlist, DataContext dataContext)
    {
        if (playlist == null) return null;

        if (playlist.Statistics == null)
            dataContext.Database.RecalculatePlaylistStatistics(playlist);

        Debug.Assert(playlist.Statistics != null);

        return new()
        {
            PlaylistId = playlist.PlaylistId,
            Publisher = ApiGameUserResponse.FromOld(playlist.Publisher, dataContext),
            Name = playlist.Name,
            IconHash = dataContext.GetIconFromHash(playlist.IconHash),
            Description = playlist.Description,
            Location = ApiGameLocationResponse.FromLocation(playlist.LocationX, playlist.LocationY)!,
            CreationDate = playlist.CreationDate,
            UpdateDate = playlist.LastUpdateDate,
            Statistics = ApiGamePlaylistStatisticsResponse.FromOld(playlist.Statistics, dataContext),
            OwnRelations = ApiGamePlaylistOwnRelationsResponse.FromOld(playlist, dataContext),
        };
    }

    public static IEnumerable<ApiGamePlaylistResponse> FromOldList(IEnumerable<GamePlaylist> oldList, DataContext dataContext) 
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}