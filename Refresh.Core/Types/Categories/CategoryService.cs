using System.Collections.Frozen;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.Core.Types.Categories.Levels;
using Refresh.Core.Types.Categories.Users;

namespace Refresh.Core.Types.Categories;

public class CategoryService : EndpointService
{
    // Level Categories
    public readonly FrozenSet<GameLevelCategory> LevelCategories;

    // ReSharper disable once InconsistentNaming
    private readonly List<GameLevelCategory> _levelCategories =
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
        new HeartedLevelsByUserCategory(),
        new QueuedLevelsByUserCategory(),

        new SearchLevelCategory(),
        new ByTagCategory(),
        new DeveloperLevelsCategory(),
        new ContestCategory(),
        new AdventureCategory(),
    ];

    // User Categories
    public readonly FrozenSet<GameUserCategory> UserCategories;

    // ReSharper disable once InconsistentNaming
    private readonly List<GameUserCategory> _userCategories =
    [
        new HeartedUsersByUserCategory(),
        new MostHeartedUsersCategory(),
        new NewestUsersCategory()
    ];

    internal CategoryService(Logger logger) : base(logger)
    {
        this.LevelCategories = this._levelCategories.ToFrozenSet();
        this.UserCategories = this._userCategories.ToFrozenSet();
    }
}