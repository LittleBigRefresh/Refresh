using System.Xml.Serialization;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Levels;

namespace Refresh.Interfaces.Game.Types.Lists;

[XmlRoot("slots")]
[XmlType("slots")]
public class SerializedMinimalLevelList : SerializedList<GameMinimalLevelResponse>
{
    public SerializedMinimalLevelList() {}
    
    public SerializedMinimalLevelList(IEnumerable<GameMinimalLevelResponse> list, int total, int skip, IEnumerable<GameUserResponse>? users = null)
    {
        this.Total = total;
        this.Items = list.ToList();
        this.Users = users?.ToList();
        this.NextPageStart = skip;
    }

    [XmlElement("slot")]
    public override List<GameMinimalLevelResponse> Items { get; set; } = null!;

    [XmlElement("user")]
    public List<GameUserResponse>? Users { get; set; }
}