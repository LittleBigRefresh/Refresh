using System.Xml.Serialization;
using Refresh.Database.Models.Photos;

namespace Refresh.Interfaces.Game.Types.Lists;

[XmlRoot("photos")]
[XmlType("photos")]
public class SerializedPhotoList
{
    public SerializedPhotoList()
    {}

    public SerializedPhotoList(IEnumerable<SerializedPhoto> items)
    {
        this.Items = items.ToList();
    }

    [XmlElement("photo")]
    public List<SerializedPhoto> Items { get; set; } = new();
}