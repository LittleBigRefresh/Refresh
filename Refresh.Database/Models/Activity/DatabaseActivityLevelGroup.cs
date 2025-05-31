using Refresh.Database.Models.Levels;

namespace Refresh.Database.Models.Activity;

public class DatabaseActivityLevelGroup : DatabaseActivityGroup
{
    public DatabaseActivityLevelGroup(GameLevel level)
    {
        this.Level = level;
    }

    public override string GroupType => "level";
    public GameLevel Level { get; set; }
}