using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Lists;

public abstract class SerializedList<TItem> : SerializedPaginationData
{
    [XmlIgnore]
    public abstract List<TItem> Items { get; set; }
}