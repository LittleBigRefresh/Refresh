using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

/// <summary>
/// Minimal favourite user list, used only by LBP PSP, since it structures things a bit differently
/// </summary>
[XmlRoot("favouriteUsers")]
[XmlType("minimalFavouriteUsers")]
public class SerializedMinimalFavouriteUserList : SerializedList<SerializedUserHandle>
{
    public SerializedMinimalFavouriteUserList() {}
    
    public SerializedMinimalFavouriteUserList(List<SerializedUserHandle> list, int count)
    {
        this.Total = count;
        this.Items = list;
    }
    
    [XmlElement("user")]
    public override List<SerializedUserHandle> Items { get; set; }
}