using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

public partial class FavouriteUserRelation : IRealmObject
{
    public GameUser UserToFavourite { get; set; }
    public GameUser UserFavouriting { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}