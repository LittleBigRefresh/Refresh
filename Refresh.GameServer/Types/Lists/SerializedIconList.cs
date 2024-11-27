using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("icons")]
[XmlType("icons")]
public class SerializedIconList 
{
    public SerializedIconList() {}

    public SerializedIconList(IEnumerable<string> iconHashes)
    {
        this.IconHashList = iconHashes.ToList();
    }

    [XmlElement("icon")]
    public List<string> IconHashList { get; set; } = new();
}