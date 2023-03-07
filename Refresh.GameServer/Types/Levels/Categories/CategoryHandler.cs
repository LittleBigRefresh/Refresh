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
            FontAwesomeIcon = "calendar",
        },
        new ByUserLevelCategory(),
        new SearchLevelCategory(),
        new LevelCategory("hearted", "favouriteSlots", true, nameof(RealmDatabaseContext.GetLevelsFavouritedByUser))
        {
            Name = "My Hearted Levels",
            Description = "Levels you've hearted",
            IconHash = "g820611",
            FontAwesomeIcon = "heart",
        },
        new LevelCategory("queued", "lolcatftw", true, nameof(RealmDatabaseContext.GetLevelsQueuedByUser))
        {
            Name = "My Queue",
            Description = "Levels you've queued",
            IconHash = "g820614",
            FontAwesomeIcon = "bell",
        },
    };
}