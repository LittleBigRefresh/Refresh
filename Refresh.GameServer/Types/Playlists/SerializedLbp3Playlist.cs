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
    [XmlElement("author")] public SerializedAuthor? Author { get; set; }
    [XmlElement("hearts")] public int HeartCount { get; set; }
    [XmlElement("levels_quota")] public int PlaylistQuota { get; set; }  // maximum number of levels in a playlist

    //[XmlArray("level_id")] public List<int> LevelIds { get; set; } = [];  // probably related to custom level order

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