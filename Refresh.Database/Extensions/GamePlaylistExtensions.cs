using Refresh.Database.Models.Playlists;

namespace Refresh.Database.Extensions;

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
        foreach (GamePlaylist parent in database.GetPlaylistsContainingPlaylistInternal(playlist).ToArray())
        {
            // Call the callback for this parent. If the callback requests to stop the traversal by returning false 
            // (for example because it has detected a loop in the tree), stop the traversal by returning
            if (!callback(playlist)) return;
            
            // Traverse all of this parent's parents
            parent.TraverseParentsRecursively(database, callback);
        }
    }

    // Limit to prevent getting stuck in infinite loops or recursions, incase our other conditions fail somehow.
    private const int MaxTraversalCount = 150;

    /// <summary>
    /// Returns true if adding the child playlist to the parent playlist will cause a loop.
    /// This is checked by looking whether the parent and child are the same playlist,
    /// whether the child playlist already has the parent playlist in its children somehwere,
    /// or whether the parent is already a child of the child playlist.
    /// </summary>
    // TODO maybe cache a flattened tree in DB so we don't have to traverse, and therefore send potentially many
    // DB calls, every time.
    public static bool DoPlaylistsContainEachOther(this GamePlaylist parent, GamePlaylist child, GameDatabaseContext database)
    {
        // Probably already validated by the caller, but make sure anyway
        if (parent.PlaylistId == child.PlaylistId)
        {
            return true;
        }
        

        // Check whether children of child have the parent
        List<int> queuedIds = [child.PlaylistId];
        List<int> alreadyTraversedIds = [];
        int currentIterationCount = 0;

        while (queuedIds.Count > 0)
        {
            // End traversal, regardless of state, if fallback iteration limit has been reached.
            currentIterationCount++;
            if (currentIterationCount >= MaxTraversalCount)
            {
                return true;
            }

            // Dequeue next ID
            int currentId = queuedIds[0];
            queuedIds.RemoveAt(0);

            // If we've found the parent in the child's sub-playlist tree, then adding will cause a loop.
            if (currentId == parent.PlaylistId)
            {
                return true;
            }

            // No need to enqueue children of already traversed playlists;
            // their sub-playlist IDs have already been added to our queue (and processed)
            // the first time we found and processed this playlist, so we don't need to do anything with it.
            // The same playlists can appear multiple times in a playlist's child tree, so we should
            // keep track of this.
            if (alreadyTraversedIds.Contains(currentId))
            {
                continue;
            }

            // Fetch and queue the child IDs. No need to cache since we already make sure we don't
            // traverse the same playlists multiple times (see above).
            queuedIds.AddRange(database.GetIdsOfAllPlaylistsInPlaylist(currentId).ToArray());
            alreadyTraversedIds.Add(currentId);
        }

        return false;
    }
}