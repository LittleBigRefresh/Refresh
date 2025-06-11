using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(LevelId), nameof(UserId))]
#endif
public partial class PlayLevelRelation : IRealmObject
{
    [ForeignKey(nameof(LevelId))]
    public GameLevel Level { get; set; }
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    [Ignored] public int LevelId { get; set; }
    [Ignored] public ObjectId UserId { get; set; }

    public DateTimeOffset Timestamp { get; set; }
    public int Count { get; set; }
}