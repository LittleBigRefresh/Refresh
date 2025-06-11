using MongoDB.Bson;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Relations;
#nullable disable

[PrimaryKey(nameof(UserToFavouriteId), nameof(UserFavouritingId))]
public partial class FavouriteUserRelation : IRealmObject
{
    [ForeignKey(nameof(UserToFavouriteId))]
    public GameUser UserToFavourite { get; set; }
    [ForeignKey(nameof(UserFavouritingId))]
    public GameUser UserFavouriting { get; set; }
    
    public ObjectId UserToFavouriteId { get; set; }
    public ObjectId UserFavouritingId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}