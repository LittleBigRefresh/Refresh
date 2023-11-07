using Bunkum.Core.Services;
using NotEnoughLogs;

namespace Refresh.GameServer.Types.Levels.Categories;

public class CategoryService : EndpointService
{
    public IEnumerable<LevelCategory> Categories => this._categories.AsReadOnly();

    // ReSharper disable once InconsistentNaming
    private readonly List<LevelCategory> _categories = new()
    {
        new CoolLevelsCategory(),
        new TeamPickedLevelsCategory(),
        
        new CurrentlyPlayingCategory(),
        new RandomLevelsCategory(),
        new NewestLevelsCategory(),
        
        new MostHeartedLevelsCategory(),
        new HighestRatedLevelsCategory(),
        new MostUniquelyPlayedLevelsCategory(),
        new MostReplayedLevelsCategory(),
        
        new ByUserLevelCategory(),
        new FavouriteSlotsByUserCategory(),
        new QueuedLevelsByUserCategory(),
        
        new SearchLevelCategory(),
        new DeveloperLevelsCategory(),
    };

    internal CategoryService(Logger logger) : base(logger)
    {
    }
}