using System.Xml.Serialization;

namespace Refresh.GameServer.Types;

public abstract class SerializedPaginationData
{
    [XmlAttribute("total")]
    public int Total { get; set; }

    [XmlAttribute("hint_start")] 
    public int NextPageStart { get; set; }
}