using Bunkum.Core;
using NotEnoughLogs;
using Refresh.Common.Exceptions;
using Refresh.Database.Models.Playlists;

namespace Refresh.Database.Extensions;

public static class GamePlaylistExtensions
{
    /// <summary>
    /// Returns whether the child playlist appears in the root playlist's sub-playlist tree, even if it's not an immediate child.
    /// Also returns true if the root and the child are the same playlist. Playlists are always compared by their IDs.
    /// </summary>
    /// <remarks>
    /// Throws TakingTooLongException if the maximum number of traversals has been reached, to prevent being stuck in an infinite loop
    /// incase our conditions somehow fail, or the sub-playlist tree is too large.
    /// </remarks>
    // TODO maybe cache a flattened tree in DB so we don't have to traverse, and therefore send potentially many
    // DB calls, every time.
    public static bool ContainsChildInTree(this GamePlaylist root, GamePlaylist child, GameDatabaseContext database, Logger? logger = null, int maxTraversalCount = 150)
    {
        // Probably already validated by the caller, but make sure anyway
        if (root.PlaylistId == child.PlaylistId)
        {
            logger?.LogDebug(BunkumCategory.UserContent, $"Root playlist {root.PlaylistId} is the same as child playlist, returning true.");
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
            if (currentIterationCount >= maxTraversalCount)
            {
                throw new TakingTooLongException($"[Traversal {currentIterationCount}/{maxTraversalCount}] "
                    + $"Maximum number of traversals reached, forcefully stopping traversal! "
                    + $"Either this traversal method has a bug causing it to not finish, or playlist #{child.PlaylistId} has too many sub-playlists!");
            }

            // Dequeue next ID
            int currentId = queuedIds[0];
            queuedIds.RemoveAt(0);

            // If we've found the parent in the child's sub-playlist tree, then adding will cause a loop.
            if (currentId == root.PlaylistId)
            {
                logger?.LogDebug(BunkumCategory.UserContent, $"[Traversal {currentIterationCount}/{maxTraversalCount}] "
                    + $"Root playlist #{root.PlaylistId} was found in child playlist #{child.PlaylistId}! Returning true");
                return true;
            }

            // No need to enqueue children of already traversed playlists;
            // their sub-playlist IDs have already been added to our queue (and processed)
            // the first time we found and processed this playlist, so we don't need to do anything with it.
            // The same playlists can appear multiple times in a playlist's child tree, so we should
            // keep track of this.
            if (alreadyTraversedIds.Contains(currentId))
            {
                logger?.LogDebug(BunkumCategory.UserContent, $"[Traversal {currentIterationCount}/{maxTraversalCount}] "
                    + $"Playlist #{currentId} has already been traversed, skipping.");
                continue;
            }

            // Fetch and queue the child IDs. No need to cache since we already make sure we don't
            // traverse the same playlists multiple times (see above).
            int[] childIds = database.GetIdsOfAllPlaylistsInPlaylist(currentId).ToArray();
            logger?.LogDebug(BunkumCategory.UserContent, $"[Traversal {currentIterationCount}/{maxTraversalCount}] "
                + $"Adding {childIds.Length} child IDs from playlist #{currentId}, and marking #{currentId} as traversed.");

            queuedIds.AddRange(childIds);
            alreadyTraversedIds.Add(currentId);
        }

        return false;
    }
}