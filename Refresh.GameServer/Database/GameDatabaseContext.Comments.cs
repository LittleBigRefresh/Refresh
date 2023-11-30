using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // Comments
{
    public GameComment PostCommentToProfile(GameUser profile, GameUser author, string content)
    {
        GameComment comment = new()
        {
            Author = author,
            Content = content,
            Timestamp = this.Time.TimestampMilliseconds,
        };
        
        this.AddSequentialObject(comment, profile.ProfileComments);
        return comment;
    }

    public IEnumerable<GameComment> GetProfileComments(GameUser profile, int count, int skip) =>
        profile.ProfileComments
            .OrderByDescending(c => c.Timestamp)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);

    public void DeleteProfileComment(GameComment comment, GameUser profile)
    {
        this.Write(() =>
        {
            profile.ProfileComments.Remove(comment);
        });
    }

    public GameComment PostCommentToLevel(GameLevel level, GameUser author, string content)
        {
            GameComment comment = new()
            {
                Author = author,
                Content = content,
                Timestamp = this.Time.TimestampMilliseconds,
            };
            
            this.AddSequentialObject(comment, level.LevelComments);
            return comment;
        }

    public IEnumerable<GameComment> GetLevelComments(GameLevel level, int count, int skip) =>
        level.LevelComments
             .OrderByDescending(c => c.Timestamp)
             .AsEnumerable()
             .Skip(skip)
             .Take(count);

    public void DeleteLevelComment(GameComment comment, GameLevel level)
    {
        this.Write(() =>
        {
            level.LevelComments.Remove(comment);
        });
    }
}