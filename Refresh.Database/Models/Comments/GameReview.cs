using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database.Models.Comments;

#nullable disable

public partial class GameReview : ISequentialId
{
    [Key] public int ReviewId { get; set; }
    
    [Required]
    public GameLevel Level { get; set;  }

    [Required]
    public GameUser Publisher { get; set; }

    #nullable enable
    
    public DateTimeOffset PostedAt { get; set; }
    
    [Obsolete("Deprecated. This attribute only exists so BackfillReviewLabelsMigration could properly migrate labels at runtime.")]
    public string? LabelsString { get; set; }
    [Required] public List<Label> Labels { get; set; } = [];

    public string Content { get; set; } = "";

    [NotMapped] public int SequentialId
    {
        get => this.ReviewId;
        set => this.ReviewId = value;
    }
}