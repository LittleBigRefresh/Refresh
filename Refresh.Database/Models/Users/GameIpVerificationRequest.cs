
using MongoDB.Bson;
#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Users;

#nullable disable

[PrimaryKey(nameof(UserId), nameof(IpAddress))]
public partial class GameIpVerificationRequest : IRealmObject
{
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    public ObjectId UserId { get; set; }
    
    public string IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}