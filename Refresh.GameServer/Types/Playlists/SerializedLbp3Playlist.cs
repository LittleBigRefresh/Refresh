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
    [XmlElement("levels")] public int LevelCount { get; set; }  // doesnt even seem to do anything
    [XmlElement("hearts")] public int HeartCount { get; set; }
    [XmlElement("levels_quota")] public int PlaylistQuota { get; set; }
    [XmlElement("icons")] public SerializedIconList? LevelIcons { get; set; }
    [XmlArray("level_id")] public List<int> LevelIds { get; set; } = [];

    public static SerializedLbp3Playlist? FromOld(GamePlaylist? old, DataContext dataContext)
    {
        if (old == null) 
            return null;

        IEnumerable<int> levelIds = [];
        IEnumerable<string> levelIcons = [];
        levelIcons = levelIcons.Append(old.IconHash);
        foreach(GameLevel level in dataContext.Database.GetLevelsInPlaylist(old, dataContext.Game))
        {
            levelIds = levelIds.Append(level.LevelId);
            levelIcons = levelIcons.Append(level.IconHash);
        }

        return new SerializedLbp3Playlist
        {
            Id = old.PlaylistId,
            Name = old.Name,
            Description = old.Description,
            Author = new SerializedAuthor(old.Publisher.Username),
            LevelCount = dataContext.Database.GetTotalLevelsInPlaylistCount(old, dataContext.Game) * -1,  // lol
            HeartCount = dataContext.Database.GetFavouriteCountForPlaylist(old),
            PlaylistQuota = UgcLimits.MaximumLevels,
            LevelIds = levelIds.ToList(),
            LevelIcons = new SerializedIconList(levelIcons),
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