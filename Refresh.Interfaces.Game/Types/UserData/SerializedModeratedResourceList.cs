using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.UserData;

#nullable disable

[XmlRoot("resources")]
public class SerializedModeratedResourceList
{
    [XmlElement("resource")]
    public List<string> Resources;
}