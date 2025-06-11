using MongoDB.Bson;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Refresh.Database.Compatibility.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Playlists;

#nullable disable

/// <summary>
/// A user-curated list of levels.
/// </summary>
public partial class GamePlaylist : IRealmObject, ISequentialId
{
    /// <summary>
    /// The unique ID of this playlist, must be > 0
    /// </summary>
    [Key, PrimaryKey] public int PlaylistId { get; set; }
    
    /// <summary>
    /// The ID of the user who published the playlist
    /// </summary>
    public ObjectId PublisherId { get; set; } 
    
    /// <summary>
    /// The user who published the playlist
    /// </summary>
    [ForeignKey(nameof(PublisherId))]
    public GameUser Publisher { get; set; }
    
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
    public string IconHash { get; set; }
    
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
    public bool IsRoot { get; set; }
    
    public int SequentialId
    {
        get => this.PlaylistId;
        set => this.PlaylistId = value;
    }
}