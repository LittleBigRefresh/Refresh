using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("updateUser")]
public class UpdateUserDataProfile : UpdateUserData {}

[XmlRoot("user")]
public class UpdateUserDataPlanets : UpdateUserData {}

[Ignored]
public class UpdateUserData
{
    [XmlElement("biography")]
    public string? Description { get; set; }
    
    [XmlElement("location")]
    public GameLocation? Location { get; set; }
    
    [XmlElement("planets")]
    public string? PlanetsHash { get; set; }
}