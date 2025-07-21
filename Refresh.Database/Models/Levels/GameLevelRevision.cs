using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Levels;

/// <summary>
/// A snapshot of a level's details at a point in time. This can be used for moderation, rollbacks of migrations, and in LBP hub's challenges.
/// </summary>
[PrimaryKey(nameof(LevelId), nameof(RevisionId))]
public class GameLevelRevision
{
    /// <summary>
    /// The level whose snapshot this is.
    /// </summary>
    [ForeignKey(nameof(LevelId))] public GameLevel Level { get; set; } = null!;
    /// <summary>
    /// The ID of the level whose snapshot this is.
    /// </summary>
    [Required] public int LevelId { get; set; }
    /// <summary>
    /// The sequential revision ID for this revision.
    /// </summary>
    [Required] public int RevisionId { get; set; }
    
    /// <summary>
    /// The point in time in which this revision was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>
    /// The user this revision was created by.
    /// </summary>
    /// <remarks>
    /// If null, this was actioned by Refresh itself.
    /// </remarks>
    [ForeignKey(nameof(CreatedById))] public GameUser? CreatedBy { get; set; }
    public ObjectId CreatedById { get; set; }
    
    public string Title { get; set; } = "";
    public string IconHash { get; set; } = "";
    public string Description { get; set; } = "";
    
    public string RootResource { get; set; } = "";
    
    public TokenGame GameVersion { get; set; }
    public GameLevelType LevelType { get; set; }
    
    public int StoryId { get; set; }
}