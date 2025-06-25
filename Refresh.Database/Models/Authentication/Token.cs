using System.Xml.Serialization;
using Bunkum.Core.Authentication;
using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Authentication;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
public partial class Token : IToken<GameUser>
{
    [Key]
    public ObjectId TokenId { get; set; } = ObjectId.GenerateNewId();
    
    // this shouldn't ever be serialized, but just in case let's ignore it
    [XmlIgnore, JsonIgnore] public string TokenData { get; set; }

    public TokenType TokenType { get; set; }
    
    public TokenPlatform TokenPlatform  { get; set; }
    public TokenGame TokenGame  { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset LoginDate { get; set; }
    
    [Required]
    public string IpAddress { get; set; }

    [Required]
    public GameUser User { get; set; }
    
    /// <summary>
    /// The digest key to use with this token, determined from the first game request created by this token
    /// </summary>
    [CanBeNull] public string Digest { get; set; }
    public bool IsHmacDigest { get; set; }
}