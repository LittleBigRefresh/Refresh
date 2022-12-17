using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Lists;

public abstract class GameList<TItem>
{
    [XmlAttribute("total")]
    public int Total { get; set; }

    [XmlAttribute("hint_start")] public int NextPageStart
    {
        get => 41;
        // ReSharper disable once ValueParameterNotUsed (will not serialize without setter)
        set {}
    }

    [XmlElement("slot")]
    public List<TItem> Items { get; set; } = new();
}