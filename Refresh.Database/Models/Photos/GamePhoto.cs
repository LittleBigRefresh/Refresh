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
    
    private const string subjectObsoleteMessage = "GamePhotoSubjects are now in their own DB table. This attribute only exists to allow migration.";

    [Obsolete(subjectObsoleteMessage)] public ObjectId? Subject1UserId { get; set; }
    [Obsolete(subjectObsoleteMessage)] public GameUser? Subject1User { get; set; }
    [Obsolete(subjectObsoleteMessage)] public string? Subject1DisplayName { get; set; }
    
    [Obsolete(subjectObsoleteMessage)] public ObjectId? Subject2UserId { get; set; }
    [Obsolete(subjectObsoleteMessage)] public GameUser? Subject2User { get; set; }
    [Obsolete(subjectObsoleteMessage)] public string? Subject2DisplayName { get; set; }
    
    [Obsolete(subjectObsoleteMessage)] public ObjectId? Subject3UserId { get; set; }
    [Obsolete(subjectObsoleteMessage)] public GameUser? Subject3User { get; set; }
    [Obsolete(subjectObsoleteMessage)] public string? Subject3DisplayName { get; set; }
    
    [Obsolete(subjectObsoleteMessage)] public ObjectId? Subject4UserId { get; set; }
    [Obsolete(subjectObsoleteMessage)] public GameUser? Subject4User { get; set; }
    [Obsolete(subjectObsoleteMessage)] public string? Subject4DisplayName { get; set; }
    
    [Obsolete(subjectObsoleteMessage)] public List<float> Subject1Bounds { get; set; } = [];
    [Obsolete(subjectObsoleteMessage)] public List<float> Subject2Bounds { get; set; } = [];
    [Obsolete(subjectObsoleteMessage)] public List<float> Subject3Bounds { get; set; } = [];
    [Obsolete(subjectObsoleteMessage)] public List<float> Subject4Bounds { get; set; } = [];
    
    #nullable disable
    
    #endregion
    
    [JsonIgnore] [NotMapped] public int SequentialId
    {
        get => this.PhotoId;
        set => this.PhotoId = value;
    }
}