using System.Xml.Serialization;
using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Playlists;

namespace Refresh.GameServer.Types.Categories.Playlists;

#nullable disable

public class SerializedPlaylistCategory : SerializedCategory
{
    [XmlElement("results")] public SerializedLbp3PlaylistList Playlists { get; set; }

    public static SerializedPlaylistCategory FromPlaylistCategory(GameCategory category)
    {
        SerializedPlaylistCategory serializedCategory = new()
        {
            Name = category.Name,
            Description = category.Description,
            Url = "/searches/playlists/" + category.ApiRoute,
            Tag = category.ApiRoute,
            Types = 
            [
                "playlist",
            ],
            IconHash = category.IconHash,
        };

        return serializedCategory;
    }

    public static SerializedPlaylistCategory FromPlaylistCategory(GamePlaylistCategory playlistCategory,
        RequestContext context,
        DataContext dataContext,
        int skip = 0,
        int count = 20)
    {
        SerializedPlaylistCategory serializedPlaylistCategory = FromPlaylistCategory(playlistCategory);

        LevelFilterSettings filterSettings = new(context, dataContext.Token!.TokenGame);
        DatabaseList<GamePlaylist> categoryPlaylists = playlistCategory.Fetch(context, skip, count, dataContext, filterSettings, dataContext.User);
        
        IEnumerable<SerializedLbp3Playlist> playlists = categoryPlaylists?.Items
            .Select(l => SerializedLbp3Playlist.FromOld(l, dataContext)) ?? [];

        serializedPlaylistCategory.Playlists = new SerializedLbp3PlaylistList
        {
            Items = playlists.ToList(),
        };

        return serializedPlaylistCategory;
    }
}