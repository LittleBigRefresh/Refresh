using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database;

public partial class GameDatabaseContext // LevelRevisions
{
    private IQueryable<GameLevelRevision> GameLevelRevisionsIncluded => this.GameLevelRevisions
        .Include(r => r.Level);

    public GameLevelRevision CreateRevisionForLevel(GameLevel level, GameUser? creator, bool save = true)
    {
        GameLevelRevision revision = new()
        {
            Level = level,
            LevelId = level.LevelId,
            CreatedAt = this._time.Now,
            CreatedBy = creator,
            CreatedById = creator?.UserId,
            
            Title = level.Title,
            Description = level.Description,
            GameVersion = level.GameVersion,
            IconHash = level.IconHash,
            RootResource = level.RootResource,
            LevelType = level.LevelType,
            StoryId = level.StoryId,
        };

        this.GameLevelRevisions.Add(revision);

        if(save)
            this.SaveChanges();

        return revision;
    }
}