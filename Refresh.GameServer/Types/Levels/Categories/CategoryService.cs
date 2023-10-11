using Bunkum.Core.Services;
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
            Description = "Levels that were most recently uploaded",
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
            Name = "My Favorites",
            Description = "Your personal list filled with your favourite levels!",
            IconHash = "g820611",
            FontAwesomeIcon = "heart",
        },
        new LevelCategory("queued", "lolcatftw", true, nameof(GameDatabaseContext.GetLevelsQueuedByUser))
        {
            Name = "My Queue",
            Description = "Levels you'd like to play!",
            IconHash = "g820614",
            FontAwesomeIcon = "bell",
        },
        new LevelCategory("mostHearted", "mostHearted", false, nameof(GameDatabaseContext.GetMostHeartedLevels))
        {
            Name = "Community's Favorites",
            Description = "The all-time most hearted levels!",
            IconHash = "g820607",
            FontAwesomeIcon = "heart",
        },
        new LevelCategory("mostLiked", new[] { "thumbs", "highestRated" }, false, nameof(GameDatabaseContext.GetHighestRatedLevels))
        {
            Name = "Liked Levels",
            Description = "The all-time most liked levels!",
            IconHash = "g820603",
            FontAwesomeIcon = "thumbs-up",
        },
        new LevelCategory("mostPlayed", "mostUniquePlays", false, nameof(GameDatabaseContext.GetMostUniquelyPlayedLevels))
        {
            Name = "Starter Pack",
            Description = "Levels that many people have played!",
            IconHash = "g820608",
            FontAwesomeIcon = "play",
        },
        new LevelCategory("mostReplayed", "mostUniquePlays", false, nameof(GameDatabaseContext.GetMostReplayedLevels))
        {
            Name = "Replayable Levels",
            Description = "Levels people love to play over and over!",
            IconHash = "g820608",
            FontAwesomeIcon = "forward",
        },
        new LevelCategory("teamPicks", "mmpicks", false, nameof(GameDatabaseContext.GetTeamPickedLevels))
        {
            Name = "Team Picks",
            Description = "Handpicked quality levels",
            IconHash = "g820626",
            FontAwesomeIcon = "certificate",
        },
        new CurrentlyPlayingCategory(),
    };

    internal CategoryService(Logger logger) : base(logger)
    {
    }
}