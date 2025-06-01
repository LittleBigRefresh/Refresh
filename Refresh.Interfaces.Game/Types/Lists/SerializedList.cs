using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Lists;

public abstract class SerializedList<TItem> : SerializedPaginationData
{
    [XmlIgnore]
    public abstract List<TItem> Items { get; set; }
}