using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Authentication;

#nullable disable

public class ResetToken : RealmObject
{
    [PrimaryKey]
    public ObjectId TokenId { get; set; } = ObjectId.GenerateNewId();
    
    [XmlIgnore] public string TokenData { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public GameUser User { get; set; }
}