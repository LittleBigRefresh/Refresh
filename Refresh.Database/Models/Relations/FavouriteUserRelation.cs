using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(UserToFavouriteId), nameof(UserFavouritingId))]
#endif
public partial class FavouriteUserRelation : IRealmObject
{
    [ForeignKey(nameof(UserToFavouriteId))]
    public GameUser UserToFavourite { get; set; }
    [ForeignKey(nameof(UserFavouritingId))]
    public GameUser UserFavouriting { get; set; }
    
    [Ignored] public ObjectId UserToFavouriteId { get; set; }
    [Ignored] public ObjectId UserFavouritingId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}