using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Types.Categories;

public class DatabaseResultList
{
    public DatabaseList<GameLevel>? Levels { get; set; } = null;
    public DatabaseList<GameUser>? Users { get; set; } = null;
    public DatabaseList<GamePlaylist>? Playlists { get; set; } = null;

    /// <summary>
    /// All items in total
    /// </summary>
    public int TotalItems => (Levels?.TotalItems ?? 0)
        + (Users?.TotalItems ?? 0)
        + (Playlists?.TotalItems ?? 0);
    
    public DatabaseResultList(DatabaseList<GameLevel> levels)
    {
        Levels = levels;
    }

    public DatabaseResultList(DatabaseList<GameUser> users)
    {
        Users = users;
    }

    public DatabaseResultList(DatabaseList<GamePlaylist> playlists)
    {
        Playlists = playlists;
    }
}