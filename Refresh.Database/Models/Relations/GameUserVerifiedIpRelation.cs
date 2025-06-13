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
    [Required]
    public GameUser User { get; set; }
    public string IpAddress { get; set; }
    
    [Required]
    [Ignored] public ObjectId UserId { get; set; }
    
    public DateTimeOffset VerifiedAt { get; set; }
}