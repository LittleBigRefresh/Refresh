using Bunkum.Core.Services;
using NotEnoughLogs;

namespace Refresh.GameServer.Types.Levels.Categories;

public class CategoryService : EndpointService
{
    public IEnumerable<LevelCategory> Categories => this._categories.AsReadOnly();

    // ReSharper disable once InconsistentNaming
    private readonly List<LevelCategory> _categories = new()
    {
        new NewestLevelsCategory(),
        new RandomLevelsCategory(),
        new ByUserLevelCategory(),
        new SearchLevelCategory(),
        new FavouriteSlotsByUserCategory(),
        new QueuedLevelsByUserCategory(),
        new MostHeartedLevelsCategory(),
        new HighestRatedLevelsCategory(),
        new MostUniquelyPlayedLevelsCategory(),
        new MostReplayedLevelsCategory(),
        new TeamPickedLevelsCategory(),
        new DeveloperLevelsCategory(),
        new CurrentlyPlayingCategory(),
        new CoolLevelsCategory(),
    };

    internal CategoryService(Logger logger) : base(logger)
    {
    }
}