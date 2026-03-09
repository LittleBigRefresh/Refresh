using MongoDB.Bson;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Photos;

#nullable disable

[JsonObject(MemberSerialization.OptOut)]
public partial class GamePhoto : ISequentialId
{
    [Key] public int PhotoId { get; set; }
    public DateTimeOffset TakenAt { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    
    [Required, ForeignKey(nameof(PublisherId))]
    public GameUser Publisher { get; set; }
    public ObjectId PublisherId { get; set; }

    #nullable restore
    [ForeignKey(nameof(LevelId))]
    public GameLevel? Level { get; set; }
    public int? LevelId { get; set; }
    #nullable disable

    public string LevelType { get; set; }
    public int OriginalLevelId { get; set; }
    public string OriginalLevelName { get; set; }
    
    [Required, ForeignKey(nameof(SmallAssetHash))]
    public GameAsset SmallAsset { get; set; }
    public string SmallAssetHash { get; set; }

    [Required, ForeignKey(nameof(MediumAssetHash))]
    public GameAsset MediumAsset { get; set; }
    public string MediumAssetHash { get; set; }

    [Required, ForeignKey(nameof(LargeAssetHash))]
    public GameAsset LargeAsset { get; set; }
    public string LargeAssetHash { get; set; }

    public string PlanHash { get; set; }

    #region Subjects

    #nullable restore
    
    //private static string subjectObsoleteMessage = "All of these are obsolete due to GamePhotoSubjects now having their own DB table, "
    //                                              +"but we have to keep these attributes for migration anyway";
    public ObjectId? Subject1UserId { get; set; }
    [ForeignKey(nameof(Subject1UserId))]
    public GameUser? Subject1User { get; set; }
    public string? Subject1DisplayName { get; set; }
    
    public ObjectId? Subject2UserId { get; set; }
    [ForeignKey(nameof(Subject2UserId))]
    public GameUser? Subject2User { get; set; }
    public string? Subject2DisplayName { get; set; }
    
    public ObjectId? Subject3UserId { get; set; }
    [ForeignKey(nameof(Subject3UserId))]
    public GameUser? Subject3User { get; set; }
    public string? Subject3DisplayName { get; set; }
    
    public ObjectId? Subject4UserId { get; set; }
    [ForeignKey(nameof(Subject4UserId))]
    public GameUser? Subject4User { get; set; }
    public string? Subject4DisplayName { get; set; }
    
    public List<float> Subject1Bounds { get; set; } = [];
    public List<float> Subject2Bounds { get; set; } = [];
    public List<float> Subject3Bounds { get; set; } = [];
    public List<float> Subject4Bounds { get; set; } = [];
    
    #nullable disable
    
    #endregion
    
    [JsonIgnore] [NotMapped] public int SequentialId
    {
        get => this.PhotoId;
        set => this.PhotoId = value;
    }
}