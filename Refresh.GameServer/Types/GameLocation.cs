using System.Xml.Serialization;
using Newtonsoft.Json;
using Realms;

namespace Refresh.GameServer.Types;

[XmlType("location")]
[JsonObject(MemberSerialization.OptIn)]
public class GameLocation : EmbeddedObject
{
    public static readonly GameLocation Zero = new()
    {
        X = 0,
        Y = 0,
    };
    
    [XmlElement("y")] [JsonProperty] public int X { get; set; }
    [XmlElement("x")] [JsonProperty] public int Y { get; set; }
}