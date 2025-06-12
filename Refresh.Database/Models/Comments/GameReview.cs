using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database.Models.Comments;

#nullable disable

public partial class GameReview : IRealmObject, ISequentialId
{
    [Key] public int ReviewId { get; set; }
    
    public GameLevel Level { get; set;  }

    public GameUser Publisher { get; set; }
    
    public DateTimeOffset PostedAt { get; set; }
    
    public string Labels { get; set; }
    
    public string Content { get; set; }
    
    [NotMapped] public int SequentialId
    {
        get => this.ReviewId;
        set => this.ReviewId = value;
    }
}