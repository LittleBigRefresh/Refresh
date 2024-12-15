using Realms;
using Refresh.GameServer.Authentication;
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
    /// The user who published the playlist
    /// </summary>
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

    public static GamePlaylist ToGamePlaylist(SerializedLbp1Playlist oldPlaylist, GameUser user, bool rootPlaylist)
    {
        return new GamePlaylist
        {
            Publisher = user, 
            Name = oldPlaylist.Name,
            Description = oldPlaylist.Description, 
            IconHash = oldPlaylist.Icon, 
            LocationX = oldPlaylist.Location.X, 
            LocationY = oldPlaylist.Location.Y,
            IsRoot = rootPlaylist,
        };
    }

    public static GamePlaylist ToGamePlaylist(SerializedLbp3Playlist oldPlaylist, GameUser user, bool rootPlaylist)
    {
        return ToGamePlaylist(oldPlaylist.Name, oldPlaylist.Description, user, rootPlaylist, TokenGame.LittleBigPlanet3);
    }

    public static GamePlaylist ToGamePlaylist(string? name, string? description, GameUser user, bool rootPlaylist, TokenGame game = TokenGame.LittleBigPlanet1)
    {
        GameLocation randomLocation = GameLocation.GetRandomLocation();
            
        return new GamePlaylist
        {
            Publisher = user, 
            Name = name ?? "",
            Description = description ?? "", 
            // if this playlist is created in lbp3, set its icon to be lbp1 Star sticker (else lbp1 Mr Molecule sticker)
            // so that if people, for example, come across a (potentially empty) playlist with a default icon in lbp1, 
            // they could more easily tell whether the playlist was made in lbp3 or not
            IconHash = (game == TokenGame.LittleBigPlanet3) ? "g18451" : "g30477",
            LocationX = randomLocation.X, 
            LocationY = randomLocation.Y,
            IsRoot = rootPlaylist,
        };
    }
}