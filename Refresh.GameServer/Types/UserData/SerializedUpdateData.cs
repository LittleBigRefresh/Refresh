using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("updateUser")]
public class SerializedUpdateDataProfile : SerializedUpdateData {}

[XmlRoot("user")]
public class SerializedUpdateDataPlanets : SerializedUpdateData {}

[Ignored]
public class SerializedUpdateData
{
    [XmlElement("biography")]
    public string? Description { get; set; }
    
    [XmlElement("location")]
    public GameLocation? Location { get; set; }
    
    [XmlElement("planets")]
    public string? PlanetsHash { get; set; }
    
    [XmlElement("icon")]
    public string? IconHash { get; set; }
}