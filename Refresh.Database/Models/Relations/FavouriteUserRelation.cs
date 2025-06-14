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
    #if POSTGRES
    [Required]
    #endif
    public GameUser UserToFavourite { get; set; }
    #if POSTGRES
    [Required]
    #endif
    [ForeignKey(nameof(UserFavouritingId))]
    public GameUser UserFavouriting { get; set; }
    
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public ObjectId UserToFavouriteId { get; set; }
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public ObjectId UserFavouritingId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}