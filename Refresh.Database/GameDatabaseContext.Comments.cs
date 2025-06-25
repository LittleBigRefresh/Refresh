using JetBrains.Annotations;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database;

public partial class GameDatabaseContext // Comments
{
    private IQueryable<GameProfileComment> GameProfileCommentsIncluded => this.GameProfileComments
        .Include(c => c.Author)
        .Include(c => c.Profile);
    
    private IQueryable<GameLevelComment> GameLevelCommentsIncluded => this.GameLevelComments
        .Include(c => c.Author)
        .Include(c => c.Level);
    
    public GameProfileComment? GetProfileCommentById(int id) => this.GameProfileCommentsIncluded
        .FirstOrDefault(c => c.SequentialId == id);
    
    public GameProfileComment PostCommentToProfile(GameUser profile, GameUser author, string content)
    {
        GameProfileComment comment = new()
        {
            Author = author,
            Profile = profile,
            Content = content,
            Timestamp = this._time.Now,
        };
        
        this.Write(() =>
        {
            this.GameProfileComments.Add(comment);
        });

        return comment;
    }

    public IEnumerable<GameProfileComment> GetProfileComments(GameUser profile, int count, int skip) =>
        this.GameProfileCommentsIncluded
            .Where(c => c.Profile == profile)
            .OrderByDescending(c => c.Timestamp)
            .AsEnumerableIfRealm()
            .Skip(skip)
            .Take(count);
    
    [Pure]
    public int GetTotalCommentsForProfile(GameUser profile) => this.GameProfileComments.Count(c => c.Profile == profile);

    public void DeleteProfileComment(GameProfileComment comment)
    {
        this.Write(() =>
        {
            this.GameProfileComments.Remove(comment);
        });
    }
    
    public GameLevelComment? GetLevelCommentById(int id) => this.GameLevelCommentsIncluded
        .FirstOrDefault(c => c.SequentialId == id);

    public GameLevelComment PostCommentToLevel(GameLevel level, GameUser author, string content)
        {
            GameLevelComment comment = new()
            {
                Author = author,
                Level = level,
                Content = content,
                Timestamp = this._time.Now,
            };
            
            this.Write(() =>
            {
                this.GameLevelComments.Add(comment);
            });
            return comment;
        }

    public IEnumerable<GameLevelComment> GetLevelComments(GameLevel level, int count, int skip) =>
        this.GameLevelCommentsIncluded
            .Where(c => c.Level == level)
            .OrderByDescending(c => c.Timestamp)
            .AsEnumerableIfRealm()
            .Skip(skip)
            .Take(count);
    
    [Pure]
    public int GetTotalCommentsForLevel(GameLevel level) => this.GameLevelComments.Count(c => c.Level == level);

    public void DeleteLevelComment(GameLevelComment comment)
    {
        this.Write(() =>
        {
            this.GameLevelComments.Remove(comment);
        });
    }
}