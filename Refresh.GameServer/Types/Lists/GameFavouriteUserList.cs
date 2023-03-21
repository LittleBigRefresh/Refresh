using System.Xml.Serialization;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("favouriteUsers")]
[XmlType("favouriteUsers")]
public class GameFavouriteUserList : GameList<GameUser>
{
    public GameFavouriteUserList() {}
    
    public GameFavouriteUserList(List<GameUser> list, int count)
    {
        this.Total = count;
        this.Items = list;
    }
    
    [XmlElement("user")]
    public override List<GameUser> Items { get; set; }
}