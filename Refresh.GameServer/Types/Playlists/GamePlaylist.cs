using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Playlists;

/// <summary>
/// A user-curated list of levels.
/// </summary>
public partial class GamePlaylist : IRealmObject, ISequentialId
{
    /// <summary>
    /// The unique ID of this playlist, must be > 0
    /// </summary>
    [PrimaryKey] public int PlaylistId { get; set; }
    
    /// <summary>
    /// The user who created the playlist
    /// </summary>
    public GameUser Creator { get; set; }
    
    /// <summary>
    /// The name of the playlist
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The description of the playlist
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// The playlist's icon, either a GUID or Hashed asset
    /// </summary>
    public string Icon { get; set; }
    
    public int LocationX { get; set; }
    public int LocationY { get; set; }

    /// <summary>
    /// The time the playlist was created
    /// </summary>
    public DateTimeOffset CreationDate { get; set; }
    /// <summary>
    /// The last time the playlist was updated. ex. name/desc/icon change, or a level/sub-playlist was added
    /// </summary>
    public DateTimeOffset LastUpdateDate { get; set; }
    
    /// <summary>
    /// Whether or not this playlist is a root playlist. This is to let us hide the root playlists when we 
    /// </summary>
    public bool RootPlaylist { get; set; }
    
    public int SequentialId
    {
        get => this.PlaylistId;
        set => this.PlaylistId = value;
    }
}