using System.Xml.Serialization;

namespace Refresh.Database.Models.Users;

#nullable disable

[XmlRoot("resources")]
public class SerializedModeratedResourceList
{
    [XmlElement("resource")]
    public List<string> Resources;
}