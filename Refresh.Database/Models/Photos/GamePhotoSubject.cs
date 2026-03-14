using Refresh.Database.Helpers;
using Refresh.Database.Models.Users;
using MongoDB.Bson;

namespace Refresh.Database.Models.Photos;

[PrimaryKey(nameof(PhotoId), nameof(PlayerId))] 
public class GamePhotoSubject
{
    [ForeignKey(nameof(UserId))]
    public GameUser? User { get; set; }
    public ObjectId? UserId { get; set; }

    [ForeignKey(nameof(PhotoId))]
    [Required] public GamePhoto Photo { get; set; } = null!;
    [Required] public int PhotoId { get; set; }

    [Required] public int PlayerId { get; set; } // player number

    public string DisplayName { get; set; } = "";
    public IList<float> Bounds { get; set; } = new float[PhotoHelper.SubjectBoundaryCount];
}