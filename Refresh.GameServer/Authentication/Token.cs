using System.Xml.Serialization;
using MongoDB.Bson;
using Newtonsoft.Json;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Authentication;

#nullable disable

public class Token : RealmObject
{
    [PrimaryKey]
    public ObjectId TokenId { get; set; } = ObjectId.GenerateNewId();
    
    // this shouldn't ever be serialized, but just in case let's ignore it
    [JsonIgnore] [XmlIgnore] public string TokenData { get; set; }
    
    public GameUser User { get; set; }
}