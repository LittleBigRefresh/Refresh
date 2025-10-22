using Refresh.Database.Models.Levels;

namespace Refresh.Database.Models.Activity;

public class DatabaseActivityLevelGroup : DatabaseActivityGroup
{
    public DatabaseActivityLevelGroup(GameLevel? level, int levelId)
    {
        this.Level = level;
        this.LevelId = levelId;
    }

    public override string GroupType => "level";
    public GameLevel? Level { get; set; }
    public int LevelId { get; set; }
}