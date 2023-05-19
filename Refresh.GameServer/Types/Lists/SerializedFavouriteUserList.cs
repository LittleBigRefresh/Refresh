using System.Xml.Serialization;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("favouriteUsers")]
[XmlType("favouriteUsers")]
public class SerializedFavouriteUserList : SerializedList<GameUser>
{
    public SerializedFavouriteUserList() {}
    
    public SerializedFavouriteUserList(List<GameUser> list, int count)
    {
        this.Total = count;
        this.Items = list;
    }
    
    [XmlElement("user")]
    public override List<GameUser> Items { get; set; }
}