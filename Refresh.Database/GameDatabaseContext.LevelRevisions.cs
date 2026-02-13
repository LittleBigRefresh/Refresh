using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database;

public partial class GameDatabaseContext // LevelRevisions
{
    private IQueryable<GameLevelRevision> GameLevelRevisionsIncluded => this.GameLevelRevisions
        .Include(r => r.Level);

    public GameLevelRevision CreateRevisionForLevel(GameLevel level, GameUser? creator, bool saveChanges = false)
    {
        // FIXME: this isn't exactly atomic, but it should be incredibly rare
        // for multiple threads to be trying to make a revision for the same level at the same time
        int sequentialId = this.GameLevelRevisions
            .Where(r => r.LevelId == level.LevelId)
            .DefaultIfEmpty()
            .Max(r => r != null ? r.RevisionId : 0);
        
        GameLevelRevision revision = new()
        {
            LevelId = level.LevelId,
            CreatedAt = this._time.Now,
            CreatedById = creator?.UserId,
            
            RevisionId = sequentialId + 1,
            
            Title = level.Title,
            Description = level.Description,
            GameVersion = level.GameVersion,
            IconHash = level.IconHash,
            RootResource = level.RootResource,
            LevelType = level.LevelType,
            StoryId = level.StoryId,
        };

        this.GameLevelRevisions.Add(revision);

        if(saveChanges)
            this.SaveChanges();

        return revision;
    }

    private IQueryable<GameLevelRevision> GetLevelRevisionsByUserInternal(GameUser user)
        => this.GameLevelRevisionsIncluded.Where(r => r.CreatedById == user.UserId);

    public DatabaseList<GameLevelRevision> GetLevelRevisionsByUser(GameUser user, int skip, int count)
        => new(this.GetLevelRevisionsByUserInternal(user), skip, count);
    
    private void UpdateLevelRevisions(IEnumerable<GameLevelRevision> revisions)
    {
        this.GameLevelRevisions.UpdateRange(revisions);
        this.SaveChanges();
    }
}