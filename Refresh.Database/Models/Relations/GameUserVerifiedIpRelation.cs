using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(UserId), nameof(IpAddress))]
#endif
public partial class GameUserVerifiedIpRelation : IRealmObject
{
    [ForeignKey(nameof(UserId))]
    #if POSTGRES
    [Required]
    #endif
    public GameUser User { get; set; }
    public string IpAddress { get; set; }
    
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public ObjectId UserId { get; set; }
    
    public DateTimeOffset VerifiedAt { get; set; }
}