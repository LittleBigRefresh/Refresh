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
        new LevelCategory("mostHearted", "mostHearted", false, nameof(GameDatabaseContext.GetMostHeartedLevels))
        {
            Name = "Most Loved",
            Description = "The most hearted levels of all time",
            IconHash = "g820607",
            FontAwesomeIcon = "heart",
        },
        new LevelCategory("mostLiked", new[] { "thumbs", "highestRated" }, false, nameof(GameDatabaseContext.GetHighestRatedLevels))
        {
            Name = "Liked Levels",
            Description = "The most liked levels of all time",
            IconHash = "g820603",
            FontAwesomeIcon = "thumbs-up",
        },
        new LevelCategory("mostPlayed", "mostUniquePlays", false, nameof(GameDatabaseContext.GetMostUniquelyPlayedLevels))
        {
            Name = "Most Played",
            Description = "The most played content",
            IconHash = "g820608",
            FontAwesomeIcon = "play",
        },
        new LevelCategory("teamPicks", "mmpicks", false, nameof(GameDatabaseContext.GetTeamPickedLevels))
        {
            Name = "Team Picks",
            Description = "The best of the best",
            IconHash = "g820626",
            FontAwesomeIcon = "certificate",
        },
        new CurrentlyPlayingCategory(),
    };

    internal CategoryService(LoggerContainer<BunkumContext> logger) : base(logger)
    {
    }
}