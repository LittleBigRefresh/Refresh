using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

public partial class FavouriteLevelRelation : IRealmObject
{
    public GameLevel Level { get; set; }
    public GameUser User { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}