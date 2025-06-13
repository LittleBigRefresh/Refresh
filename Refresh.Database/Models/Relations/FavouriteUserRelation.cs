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
    [Required]
    public GameUser UserToFavourite { get; set; }
    [Required]
    [ForeignKey(nameof(UserFavouritingId))]
    public GameUser UserFavouriting { get; set; }
    
    [Required]
    [Ignored] public ObjectId UserToFavouriteId { get; set; }
    [Required]
    [Ignored] public ObjectId UserFavouritingId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}