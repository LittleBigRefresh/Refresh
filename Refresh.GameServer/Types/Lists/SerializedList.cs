using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Lists;

public abstract class SerializedList<TItem>
{
    [XmlAttribute("total")]
    public int Total { get; set; }

    [XmlAttribute("hint_start")] public int NextPageStart
    {
        get => this.Items.Count + 1;
        // ReSharper disable once ValueParameterNotUsed (will not serialize without setter)
        set {}
    }
    
    [XmlIgnore]
    public abstract List<TItem> Items { get; set; }
}