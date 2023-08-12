using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Comments
{
    public GameComment PostCommentToProfile(GameUser profile, GameUser author, string content)
    {
        GameComment comment = new()
        {
            Author = author,
            Content = content,
            Timestamp = this._time.TimestampMilliseconds,
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
}