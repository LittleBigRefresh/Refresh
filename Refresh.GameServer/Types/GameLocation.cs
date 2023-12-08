using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using Realms;

namespace Refresh.GameServer.Types;

[XmlType("location")]
[JsonObject(MemberSerialization.OptIn)]
[Keyless] // TODO: fix
public partial class GameLocation : IEmbeddedObject
{
    public static GameLocation Zero => new()
    {
        X = 0,
        Y = 0,
    };
    
    [XmlElement("y")] [JsonProperty] public int X { get; set; }
    [XmlElement("x")] [JsonProperty] public int Y { get; set; }
}