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

    // Limit to prevent getting stuck in infinite loops or recursions
    private static const int MaxTraversalCount = 150;

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
            return true;
        
        // Incase we encounter the same playlists multiple times per traversal, which is possible.
        Dictionary<int, int[]> cachedChildren = [];
        Dictionary<int, int[]> cachedParents = [];

        // Check whether children of child have the parent
        List<int> queuedIds = [child.PlaylistId];
        int currentIterationCount = 0;
        while (queuedIds.Length > 0)
        {
            currentIterationCount++;
            if (currentIterationCount >= MaxTraversalCount)
            {
                return true;
            }

            int currentId = queuedIds[0];
            if (currentId == parent.PlaylistId)
            {
                return true;
            }

            int[] childIds = cachedChildren.GetValueOrDefault(currentId)
                ?? database.GetIdsOfAllPlaylistsInPlaylist(currentId).ToArray();

            cachedChildren.Add(currentId, childIds);
            queuedIds.Concat(childIds);
            queuedIds.Remove(0);
        }

        // Check whether parent is a descendant of child
        queuedIds = [child.PlaylistId];
        currentIterationCount = 0;
        while (queuedIds.Length > 0)
        {
            currentIterationCount++;
            if (currentIterationCount >= maxIterationCount)
            {
                return true;
            }

            int currentId = queuedIds[0];
            if (currentId == parent.PlaylistId)
            {
                return true;
            }

            int[]? childIds = cachedChildren.GetValueOrDefault(currentId);

            // If we couldn't find in cache, get from DB and add to cache
            if (childIds == null)
            {
                database.GetIdsOfAllPlaylistsInPlaylist(currentId).ToArray();
                cachedChildren.Add(currentId, childIds);
            }
                
            queuedIds.Concat(childIds);
            queuedIds.Remove(0);
        }

        // If the parent contains the child in its parent tree, block the request to prevent recursive playlists
            bool recursive = false;
            IEnumerable<GamePlaylist> traversedPlaylists = [childPlaylist];
            parentPlaylist.TraverseParentsRecursively(database, delegate(GamePlaylist playlist)
            {
                // If we have already traversed this playlist before, we have found a loop. Stop traversing in that case.
                if (traversedPlaylists.Contains(playlist))
                {
                    recursive = true;
                    return false;
                }
                else
                {
                    // Remember this playlist for loop detection
                    traversedPlaylists = traversedPlaylists.Append(playlist);
                    return true;
                }
            });
            if (recursive) {
                // If adding this playlist to its parent has caused a loop which was not there before, remove it from its parent
                database.RemovePlaylistFromPlaylist(childPlaylist, parentPlaylist);
                return BadRequest;
            }
    }
}