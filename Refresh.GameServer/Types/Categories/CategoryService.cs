using System.Collections.Frozen;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.GameServer.Types.Categories.Levels;
using Refresh.GameServer.Types.Categories.Playlists;
using Refresh.GameServer.Types.Categories.Users;

namespace Refresh.GameServer.Types.Categories;

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

    // Playlist Categories
    public readonly FrozenSet<GamePlaylistCategory> PlaylistCategories;

    // ReSharper disable once InconsistentNaming
    private readonly List<GamePlaylistCategory> _playlistCategories =
    [
        new NewestPlaylistsCategory(),
        new HeartedPlaylistsByUserCategory(),
        new ByUserPlaylistCategory(),
        new MostHeartedPlaylistsCategory(),
    ];

    // User Categories
    public readonly FrozenSet<GameUserCategory> UserCategories;

    // ReSharper disable once InconsistentNaming
    private readonly List<GameUserCategory> _userCategories =
    [
        new NewestUsersCategory(),
        new HeartedUsersByUserCategory(),
        new MutualsOfUserCategory(),
        new MostHeartedUsersCategory(),
    ];

    internal CategoryService(Logger logger) : base(logger)
    {
        this.LevelCategories = this._levelCategories.ToFrozenSet();
        this.PlaylistCategories = this._playlistCategories.ToFrozenSet();
        this.UserCategories = this._userCategories.ToFrozenSet();
    }
}