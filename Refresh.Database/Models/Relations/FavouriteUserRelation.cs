using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

[PrimaryKey(nameof(UserToFavouriteId), nameof(UserFavouritingId))]
public partial class FavouriteUserRelation
{
    [ForeignKey(nameof(UserToFavouriteId))]
    [Required]
    public GameUser UserToFavourite { get; set; }
    [Required]
    [ForeignKey(nameof(UserFavouritingId))]
    public GameUser UserFavouriting { get; set; }
    
    [Required] public ObjectId UserToFavouriteId { get; set; }
    [Required] public ObjectId UserFavouritingId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}