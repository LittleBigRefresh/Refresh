using System.Xml.Serialization;
using Refresh.Database.Models;
using Refresh.Database.Query;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Types.Playlists;

#nullable disable

[XmlType("playlist")]
[XmlRoot("playlist")]
public class SerializedLbp1Playlist : IDataConvertableFrom<SerializedLbp1Playlist, Database.Models.Playlists.GamePlaylist>, ISerializedCreatePlaylistInfo
{
    [XmlElement("id")]
    public int Id { get; set; }
    
    [XmlElement("name")]
    public string Name { get; set; } = null!;
    [XmlElement("description")]
    public string Description { get; set; } = null!;
    [XmlElement("icon")]
    public string Icon { get; set; }
    
    [XmlElement("location")]
    public GameLocation Location { get; set; }
    
    #nullable enable

    public static SerializedLbp1Playlist? FromOld(Database.Models.Playlists.GamePlaylist? old, DataContext dataContext)
    {
        if (old == null) 
            return null;
        
        return new SerializedLbp1Playlist
        {
            Id = old.PlaylistId,
            Name = old.Name,
            Description = old.Description,
            Icon = old.IconHash,
            Location = new GameLocation(old.LocationX, old.LocationY),
        };
    }

    public static IEnumerable<SerializedLbp1Playlist> FromOldList(IEnumerable<Database.Models.Playlists.GamePlaylist> oldList, DataContext dataContext)
        => oldList.Select(p => FromOld(p, dataContext)!);
}