using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Reviews;

[XmlRoot("deleted_by")]
public enum ReviewDeletedBy
{
    [XmlEnum(Name = "none")]
    None,
}