using System.Collections.Frozen;
using Bunkum.Core.Services;
using NotEnoughLogs;

namespace Refresh.GameServer.Types.Levels.Categories;

public class CategoryService : EndpointService
{
    public readonly FrozenSet<LevelCategory> Categories;

    // ReSharper disable once InconsistentNaming
    private readonly List<LevelCategory> _categories =
    [
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
        new ByTagCategory(),
        new DeveloperLevelsCategory(),
        new ContestCategory(),
    ];

    internal CategoryService(Logger logger) : base(logger)
    {
        this.Categories = this._categories.ToFrozenSet();
    }
}