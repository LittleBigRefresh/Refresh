using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;

namespace Refresh.GameServer.Database.Types;

[XmlType("location")]
public class GameLocation : RealmObject
{
    [XmlIgnore]
    public ObjectId LocationId { get; set; } = ObjectId.GenerateNewId();
    
    [XmlElement("y")]
    public int X { get; set; }
    [XmlElement("x")]
    public int Y { get; set; }
}