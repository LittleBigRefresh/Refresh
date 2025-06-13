using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(LevelId), nameof(UserId))]
#endif
public partial class FavouriteLevelRelation : IRealmObject
{
    [Required]
    [ForeignKey(nameof(LevelId))]
    public GameLevel Level { get; set; }
    [Required]
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    [Required]
    [Ignored] public int LevelId { get; set; }
    [Required]
    [Ignored] public ObjectId UserId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}