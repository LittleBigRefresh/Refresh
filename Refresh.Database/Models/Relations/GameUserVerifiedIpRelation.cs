using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(UserId), nameof(IpAddress))]
public partial class GameUserVerifiedIpRelation
{
    [ForeignKey(nameof(UserId)), Required]
    public GameUser User { get; set; }
    public string IpAddress { get; set; }
    
    [Required] public ObjectId UserId { get; set; }
    
    public DateTimeOffset VerifiedAt { get; set; }
}