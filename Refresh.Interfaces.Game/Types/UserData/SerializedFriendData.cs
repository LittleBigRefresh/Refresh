using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.UserData;

#nullable disable

[Serializable, XmlRoot("npdata"), XmlType("npdata")]
public class SerializedFriendData
{
    [XmlElement("friends")]
    public SerializedHandleList FriendsList { get; set; }
    
    [XmlElement("blocked")]
    public SerializedHandleList BlockList { get; set; }

    public class SerializedHandleList
    {
        [XmlElement("npHandle")]
        public List<string> Names { get; set; }
    }
}