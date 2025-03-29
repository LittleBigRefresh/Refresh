using System.Xml.Serialization;
using Refresh.GameServer.Types.Playlists;

namespace Refresh.GameServer.Types.Lists.Results;

[XmlRoot("results")]
[XmlType("results")]
public class SerializedLbp3PlaylistResultsList : SerializedLbp3PlaylistList
{
    public SerializedLbp3PlaylistResultsList() {}
    
    public SerializedLbp3PlaylistResultsList(IEnumerable<SerializedLbp3Playlist>? list, int? nextPageIndex, int? totalItems)
    {
        this.Items = list?.ToList() ?? [];
        this.NextPageStart = nextPageIndex ?? -1;
        this.Total = totalItems ?? 0;
    }
}