using System.Xml.Serialization;
using Bunkum.HttpServer.Authentication;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Authentication;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
public partial class Token : IRealmObject, IToken
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
    
    internal int _TokenPlatform { get; set; }
    internal int _TokenGame { get; set; }
    
    public TokenPlatform TokenPlatform 
    {
        get => (TokenPlatform)this._TokenPlatform;
        set => this._TokenPlatform = (int)value;
    }
    public TokenGame TokenGame 
    {
        get => (TokenGame)this._TokenGame;
        set => this._TokenGame = (int)value;
    }

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset LoginDate { get; set; }

    public GameUser User { get; set; }
}