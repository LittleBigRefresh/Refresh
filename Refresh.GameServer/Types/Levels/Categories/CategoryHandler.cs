using Refresh.GameServer.Database;

namespace Refresh.GameServer.Types.Levels.Categories;

public static class CategoryHandler
{
    public static IEnumerable<LevelCategory> Categories => _categories.AsReadOnly();

    // ReSharper disable once InconsistentNaming
    private static readonly List<LevelCategory> _categories = new()
    {
        new LevelCategory("newest", "newest", false, nameof(RealmDatabaseContext.GetNewestLevels))
        {
            Name = "Newest Levels",
            Description = "Levels that were uploaded recently",
            IconHash = "g820623",
        },
        new ByUserLevelCategory(),
    };
}