using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Playlists;

namespace Refresh.GameServer.Extensions;

public static class GamePlaylistExtensions
{
    /// <summary>
    /// Recursively traverse the parent playlists of this playlist
    /// </summary>
    /// <param name="playlist">The root playlist</param>
    /// <param name="database">The database, used to retrieve playlist info</param>
    /// <param name="callback">Callback run on every playlist in the parent tree</param>
    public static void TraverseParentsRecursively(this GamePlaylist playlist, GameDatabaseContext database,
        Action<GamePlaylist> callback)
    {
        // Iterate over all parents
        foreach (GamePlaylist parent in database.GetPlaylistsContainingPlaylist(playlist))
        {
            // Call the callback for this parent
            callback(playlist);
            
            // Traverse all of this parent's parents
            parent.TraverseParentsRecursively(database, callback);
        }
    }

}