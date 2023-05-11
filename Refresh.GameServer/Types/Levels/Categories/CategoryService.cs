using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Types.Levels.Categories;

public class CategoryService : EndpointService
{
    public IEnumerable<LevelCategory> Categories => this._categories.AsReadOnly();

    // ReSharper disable once InconsistentNaming
    private readonly List<LevelCategory> _categories = new()
    {
        new LevelCategory("newest", "newest", false, nameof(GameDatabaseContext.GetNewestLevels))
        {
            Name = "Newest Levels",
            Description = "Levels that were uploaded recently",
            IconHash = "g820623",
            FontAwesomeIcon = "calendar",
        },
        new LevelCategory("random", "lbp2luckydip", false, nameof(GameDatabaseContext.GetRandomLevels))
        {
            Name = "Lucky Dip",
            Description = "A random assortment of levels!",
            IconHash = "g820605",
            FontAwesomeIcon = "shuffle",
        },
        new ByUserLevelCategory(),
        new SearchLevelCategory(),
        new LevelCategory("hearted", "favouriteSlots", true, nameof(GameDatabaseContext.GetLevelsFavouritedByUser))
        {
            Name = "My Hearted Levels",
            Description = "Levels you've hearted",
            IconHash = "g820611",
            FontAwesomeIcon = "heart",
        },
        new LevelCategory("queued", "lolcatftw", true, nameof(GameDatabaseContext.GetLevelsQueuedByUser))
        {
            Name = "My Queue",
            Description = "Levels you've queued",
            IconHash = "g820614",
            FontAwesomeIcon = "bell",
        },
    };

    internal CategoryService(LoggerContainer<BunkumContext> logger) : base(logger)
    {
    }
}