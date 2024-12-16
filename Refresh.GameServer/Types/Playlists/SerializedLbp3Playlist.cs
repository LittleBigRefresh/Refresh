using System.Xml.Serialization;
using Refresh.Common.Constants;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;

namespace Refresh.GameServer.Types.Playlists;

[XmlRoot("playlist")]
[XmlType("playlist")]
public class SerializedLbp3Playlist : IDataConvertableFrom<SerializedLbp3Playlist, GamePlaylist>
{
    [XmlElement("id")] public int Id { get; set; }
    [XmlElement("name")] public string? Name { get; set; }
    [XmlElement("description")] public string? Description { get; set; }
    /// <summary>
    /// Object containing the NpHandle (username) of who created this playlist
    /// </summary>
    [XmlElement("author")] public SerializedAuthor? Author { get; set; }
    /// <summary>
    /// Amount of times this playlist has been hearted
    /// </summary>
    [XmlElement("hearts")] public int HeartCount { get; set; }
    /// <summary>
    /// Maximum number of levels lbp3 will allow to be added into this playlist
    /// </summary>
    [XmlElement("levels_quota")] public int PlaylistQuota { get; set; }

    public static SerializedLbp3Playlist? FromOld(GamePlaylist? old, DataContext dataContext)
    {
        if (old == null) 
            return null;

        return new SerializedLbp3Playlist
        {
            Id = old.PlaylistId,
            Name = old.Name,
            Description = old.Description,
            Author = new SerializedAuthor(old.Publisher.Username),
            HeartCount = dataContext.Database.GetFavouriteCountForPlaylist(old),
            PlaylistQuota = UgcLimits.MaximumLevels,
        };
    }

    public static IEnumerable<SerializedLbp3Playlist> FromOldList(IEnumerable<GamePlaylist> oldList, DataContext dataContext)
        => oldList.Select(p => FromOld(p, dataContext)!);

        
    // elbeppe 3 moment
    [XmlRoot("author")]
    [XmlType("author")]
    public class SerializedAuthor
    {
        public SerializedAuthor() {}
        public SerializedAuthor(string username)
        {
            this.Username = username;
        }
        [XmlElement("npHandle")] public string Username { get; set; } = SystemUsers.UnknownUserName;
    }
}