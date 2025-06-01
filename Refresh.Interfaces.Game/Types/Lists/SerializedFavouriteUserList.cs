using System.Xml.Serialization;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;

namespace Refresh.Interfaces.Game.Types.Lists;

#nullable disable

[XmlRoot("favouriteUsers")]
[XmlType("favouriteUsers")]
public class SerializedFavouriteUserList : SerializedList<GameUserResponse>
{
    public SerializedFavouriteUserList() {}
    
    public SerializedFavouriteUserList(List<GameUserResponse> list, int total, int skip)
    {
        this.Total = total;
        this.Items = list;
        this.NextPageStart = skip + 1;
    }
    
    [XmlElement("user")]
    public override List<GameUserResponse> Items { get; set; }
}