using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Activity;

public class DatabaseActivityPage
{
    public DateTimeOffset Start;
    public DateTimeOffset End;
    
    public List<DatabaseActivityGroup> EventGroups = [];

    public List<GameUser> Users = [];
    public List<GameLevel> Levels = [];
    public List<GameSubmittedScore> Scores = [];
    public List<GamePhoto> Photos = [];
}