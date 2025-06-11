using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(LevelId), nameof(UserId))]
public partial class QueueLevelRelation : IRealmObject
{
    [ForeignKey(nameof(LevelId))]
    public GameLevel Level { get; set; }
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    public int LevelId { get; set; }
    public ObjectId UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}