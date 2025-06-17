using System.Xml.Serialization;
using Refresh.Common.Constants;
using Refresh.Core.Types.Data;
using Refresh.Database.Models;
using Refresh.Database.Query;

namespace Refresh.Interfaces.Game.Types.Playlists;

// Disable false nullability warnings caused by Realm's source generators.
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

[XmlRoot("playlist")]
[XmlType("playlist")]
public class SerializedLbp3Playlist : IDataConvertableFrom<SerializedLbp3Playlist, Database.Models.Playlists.GamePlaylist>, ISerializedCreatePlaylistInfo
{
    [XmlElement("id")] public int Id { get; set; }

    [XmlElement("name")] public string? Name { get; set; }
    [XmlElement("description")] public string? Description { get; set; }

    /// <summary>
    /// Object containing the NpHandle (username) of who created this playlist
    /// </summary>
    [XmlElement("author")] public SerializedLbp3PlaylistAuthor? Author { get; set; }
    /// <summary>
    /// Amount of times this playlist has been hearted
    /// </summary>
    [XmlElement("hearts")] public int HeartCount { get; set; }
    /// <summary>
    /// Maximum number of levels lbp3 will allow to be added into this playlist
    /// </summary>
    [XmlElement("levels_quota")] public int PlaylistQuota { get; set; }

    public string? Icon => null;
    public GameLocation? Location => null;

    public static SerializedLbp3Playlist? FromOld(Database.Models.Playlists.GamePlaylist? old, DataContext dataContext)
    {
        if (old == null) 
            return null;

        return new SerializedLbp3Playlist
        {
            Id = old.PlaylistId,
            Name = old.Name,
            Description = old.Description,
            Author = new SerializedLbp3PlaylistAuthor
            {
                Username = old.Publisher.Username
            },
            HeartCount = dataContext.Database.GetFavouriteCountForPlaylist(old),
            PlaylistQuota = UgcLimits.MaximumLevels,
        };
    }

    public static IEnumerable<SerializedLbp3Playlist> FromOldList(IEnumerable<Database.Models.Playlists.GamePlaylist> oldList, DataContext dataContext)
        => oldList.Select(p => FromOld(p, dataContext)!);
}