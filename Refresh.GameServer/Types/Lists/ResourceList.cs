using System.Xml.Serialization;
using Refresh.GameServer.Types.Comments;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("resources")]
[XmlType("resources")]
public class ResourceList
{
    public ResourceList() {}
    
    public ResourceList(IEnumerable<string> resources)
    {
        this.Items = resources.ToList();
    }

    [XmlElement("resource")]
    public List<string> Items { get; set; } = new();
}