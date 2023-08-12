using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("favouriteUsers")]
[XmlType("favouriteUsers")]
public class SerializedFavouriteUserList : SerializedList<GameUserResponse>
{
    public SerializedFavouriteUserList() {}
    
    public SerializedFavouriteUserList(List<GameUserResponse> list, int count)
    {
        this.Total = count;
        this.Items = list;
    }
    
    [XmlElement("user")]
    public override List<GameUserResponse> Items { get; set; }
}