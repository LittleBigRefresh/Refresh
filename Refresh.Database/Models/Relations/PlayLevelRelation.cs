using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class PlayLevelRelation : IRealmObject
{
    [Key] [Ignored] public ObjectId PlayId { get; set; } = ObjectId.GenerateNewId();
    
    [ForeignKey(nameof(LevelId))]
    #if POSTGRES
    [Required]
    #endif
    public GameLevel Level { get; set; }
    [ForeignKey(nameof(UserId))]
    #if POSTGRES
    [Required]
    #endif
    public GameUser User { get; set; }
    
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public int LevelId { get; set; }
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public ObjectId UserId { get; set; }

    public DateTimeOffset Timestamp { get; set; }
    public int Count { get; set; }
}