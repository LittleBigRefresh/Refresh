using Refresh.Database;
using Refresh.Database.Models.Playlists;

namespace Refresh.Core.Extensions;

public static class GamePlaylistExtensions
{
    /// <summary>
    /// Recursively traverse the parent playlists of this playlist
    /// </summary>
    /// <param name="playlist">The root playlist</param>
    /// <param name="database">The database, used to retrieve playlist info</param>
    /// <param name="callback">Callback run on every playlist in the parent tree. If it returns false, stop traversing</param>
    public static void TraverseParentsRecursively(this GamePlaylist playlist, GameDatabaseContext database,
        Func<GamePlaylist, bool> callback)
    {
        // Iterate over all parents
        foreach (GamePlaylist parent in database.GetPlaylistsContainingPlaylist(playlist))
        {
            // Call the callback for this parent. If the callback requests to stop the traversal by returning false 
            // (for example because it has detected a loop in the tree), stop the traversal by returning
            if (!callback(playlist)) return;
            
            // Traverse all of this parent's parents
            parent.TraverseParentsRecursively(database, callback);
        }
    }
}