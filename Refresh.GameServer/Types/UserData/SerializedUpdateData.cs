using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Endpoints.Game.DataTypes.Request;

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
    
    [XmlElement("yay2")]
    public string? YayFaceHash { get; set; }
    
    [XmlElement("boo2")]
    public string? BooFaceHash { get; set; }

    [XmlElement("meh2")]
    public string? MehFaceHash { get; set; }

    [XmlArray("slots")]
    public List<GameLevelRequest>? Levels { get; set; }
}