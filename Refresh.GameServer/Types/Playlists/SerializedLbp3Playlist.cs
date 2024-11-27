using System.Xml.Serialization;
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
    [XmlElement("author")] public SerializedAuthor? Author { get; set; }
    [XmlElement("levels")] public int LevelCount { get; set; }
    [XmlElement("hearts")] public int HeartCount { get; set; }
    // [XmlElement("icons")] public SerializedIconList? LevelIcons { get; set; }
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
            LevelCount = dataContext.Database.GetTotalLevelsInPlaylistCount(old, dataContext.Game),
            HeartCount = dataContext.Database.GetFavouriteCountForPlaylist(old),
            PlaylistQuota = 187,   // experimenting lo
        };
    }

    public static IEnumerable<SerializedLbp3Playlist> FromOldList(IEnumerable<GamePlaylist> oldList, DataContext dataContext)
        => oldList.Select(p => FromOld(p, dataContext)!);

        

    // nonsense smh
    [XmlRoot("author")]
    [XmlType("author")]
    public class SerializedAuthor
    {
        public SerializedAuthor() {}
        public SerializedAuthor(string username)
        {
            this.Username = username;
        }
        [XmlElement("npHandle")] public string Username { get; set; } = "";
    }
}