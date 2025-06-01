using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Reviews;

[XmlRoot("deleted_by")]
public enum ReviewDeletedBy
{
    [XmlEnum(Name = "none")]
    None,
}