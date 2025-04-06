using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Playlists;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("results")]
[XmlType("results")]
public class SerializedCategoryResultsList : SerializedPaginationData
{
    public SerializedCategoryResultsList() {}

    public SerializedCategoryResultsList(IEnumerable<GameMinimalLevelResponse>? levels, int? nextPageIndex, int? totalItems)
    {
        this.Levels = levels?.ToList() ?? [];
        this.NextPageStart = nextPageIndex ?? -1;
        this.Total = totalItems ?? 0;
    }
    
    public SerializedCategoryResultsList(IEnumerable<SerializedLbp3Playlist>? playlists, int? nextPageIndex, int? totalItems)
    {
        this.Playlists = playlists?.ToList() ?? [];
        this.NextPageStart = nextPageIndex ?? -1;
        this.Total = totalItems ?? 0;
    }

    public SerializedCategoryResultsList(IEnumerable<GameUserResponse>? users, int? nextPageIndex, int? totalItems)
    {
        this.Users = users?.ToList() ?? [];
        this.NextPageStart = nextPageIndex ?? -1;
        this.Total = totalItems ?? 0;
    }

    public SerializedCategoryResultsList(IEnumerable<GameMinimalLevelResponse>? levels, IEnumerable<SerializedLbp3Playlist>? playlists, IEnumerable<GameUserResponse>? users, int? nextPageIndex, int? totalItems)
    {
        this.Levels = levels?.ToList() ?? [];
        this.Playlists = playlists?.ToList() ?? [];
        this.Users = users?.ToList() ?? [];
        this.NextPageStart = nextPageIndex ?? -1;
        this.Total = totalItems ?? 0;
    }

    [XmlElement("slot")] public List<GameMinimalLevelResponse> Levels { get; set; } = [];
    [XmlElement("playlist")] public List<SerializedLbp3Playlist> Playlists { get; set; } = [];
    [XmlElement("user")] public List<GameUserResponse> Users { get; set; } = [];
}