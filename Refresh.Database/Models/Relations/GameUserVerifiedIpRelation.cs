using MongoDB.Bson;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(UserId), nameof(IpAddress))]
public partial class GameUserVerifiedIpRelation : IRealmObject
{
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    public string IpAddress { get; set; }
    
    public ObjectId UserId { get; set; }
    
    public DateTimeOffset VerifiedAt { get; set; }
}