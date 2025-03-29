using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;

namespace Refresh.GameServer.Types.Lists.Results;

[XmlRoot("results")]
[XmlType("results")]
public class SerializedUserResultsList : SerializedUserList
{
    public SerializedUserResultsList() {}
    
    public SerializedUserResultsList(IEnumerable<GameUserResponse>? list, int? nextPageIndex, int? totalItems)
    {
        this.Items = list?.ToList() ?? [];
        this.NextPageStart = nextPageIndex ?? -1;
        this.Total = totalItems ?? 0;
    }
}