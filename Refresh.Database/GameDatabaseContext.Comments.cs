using JetBrains.Annotations;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database;

public partial class GameDatabaseContext // Comments
{
    public GameProfileComment? GetProfileCommentById(int id) =>
        this.GameProfileComments.FirstOrDefault(c => c.SequentialId == id);
    
    public GameProfileComment PostCommentToProfile(GameUser profile, GameUser author, string content)
    {
        GameProfileComment comment = new()
        {
            Author = author,
            Profile = profile,
            Content = content,
            Timestamp = this._time.Now,
        };
        
        this.AddSequentialObject(comment);
        return comment;
    }

    public IEnumerable<GameProfileComment> GetProfileComments(GameUser profile, int count, int skip) =>
        this.GameProfileComments
            .Where(c => c.Profile == profile)
            .OrderByDescending(c => c.Timestamp)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
    
    [Pure]
    public int GetTotalCommentsForProfile(GameUser profile) => this.GameProfileComments.Count(c => c.Profile == profile);

    public void DeleteProfileComment(GameProfileComment comment, GameUser profile)
    {
        this.Write(() =>
        {
            this.GameProfileComments.Remove(comment);
        });
    }
    
    public GameLevelComment? GetLevelCommentById(int id) =>
        this.GameLevelComments.FirstOrDefault(c => c.SequentialId == id);

    public GameLevelComment PostCommentToLevel(GameLevel level, GameUser author, string content)
        {
            GameLevelComment comment = new()
            {
                Author = author,
                Level = level,
                Content = content,
                Timestamp = this._time.Now,
            };
            
            this.AddSequentialObject(comment);
            return comment;
        }

    public IEnumerable<GameLevelComment> GetLevelComments(GameLevel level, int count, int skip) =>
        this.GameLevelComments
            .Where(c => c.Level == level)
            .OrderByDescending(c => c.Timestamp)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
    
    [Pure]
    public int GetTotalCommentsForLevel(GameLevel level) => this.GameLevelComments.Count(c => c.Level == level);

    public void DeleteLevelComment(GameLevelComment comment, GameLevel level)
    {
        this.Write(() =>
        {
            this.GameLevelComments.Remove(comment);
        });
    }
}