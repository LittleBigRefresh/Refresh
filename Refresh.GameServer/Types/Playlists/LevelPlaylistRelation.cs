using Realms;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Playlists;

/// <summary>
/// A mapping of playlist -> sub-level
/// </summary>
public partial class LevelPlaylistRelation : IRealmObject
{
    /// <summary>
    /// The playlist the level is contained in
    /// </summary>
        public int PlaylistId { get; set; }
    /// <summary>
    /// The level contained within the playlist
    /// </summary>
    public GameLevel Level { get; set; }
}