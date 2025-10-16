using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.UserData;

[XmlRoot("clientsConnected")]
[XmlType("clientsConnected")]
public class SerializedClientsConnected
{
    // Just set all to true, we don't track this data and probably don't want to either.
    [XmlElement("lbp1")] public bool LBP1 { get; set; } = true;
    [XmlElement("lbp2")] public bool LBP2 { get; set; } = true;
    [XmlElement("lbp3ps3")] public bool LBP3PS3 { get; set; } = true;
    [XmlElement("lbp3ps4")] public bool LBP3PS4 { get; set; } = true;
    [XmlElement("lbpme")] public bool LBPme { get; set; } = true;
}