using System.Xml.Serialization;
using MongoDB.Bson;
using Newtonsoft.Json;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Authentication;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
public class Token : RealmObject
{
    [PrimaryKey]
    public ObjectId TokenId { get; set; } = ObjectId.GenerateNewId();
    
    // this shouldn't ever be serialized, but just in case let's ignore it
    [XmlIgnore] public string TokenData { get; set; }
    
    // Realm can't store enums, use recommended workaround
    // ReSharper disable once InconsistentNaming (can't fix due to conflict with TokenType)
    internal int _TokenType { get; set; }

    public TokenType TokenType
    {
        get => (TokenType)this._TokenType;
        set => this._TokenType = (int)value;
    }

    public DateTimeOffset ExpiresAt { get; set; }

    public GameUser User { get; set; }
}