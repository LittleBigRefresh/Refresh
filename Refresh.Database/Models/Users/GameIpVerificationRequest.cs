using MongoDB.Bson;

namespace Refresh.Database.Models.Users;

#nullable disable
#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(UserId), nameof(IpAddress))]
#endif
public partial class GameIpVerificationRequest : IRealmObject
{
    [ForeignKey(nameof(UserId))]
    #if POSTGRES
    [Required]
    #endif
    public GameUser User { get; set; }
    
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public ObjectId UserId { get; set; }
    
    public string IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}