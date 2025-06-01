using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types;

public abstract class SerializedPaginationData
{
    [XmlAttribute("total")]
    public int Total { get; set; }

    [XmlAttribute("hint_start")] 
    public int NextPageStart { get; set; }
}