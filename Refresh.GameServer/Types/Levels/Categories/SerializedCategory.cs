using System.Xml.Serialization;
using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

#nullable disable

[XmlType("category")]
public class SerializedCategory
{
    [XmlElement("name")]
    public string Name { get; set; }
    [XmlElement("description")]
    public string Description { get; set; }
    [XmlElement("url")]
    public string Url { get; set; }
    [XmlElement("tag")]
    public string Tag { get; set; }
    [XmlElement("icon")]
    public string IconHash { get; set; }
    
    [XmlElement("results")]
    public SerializedMinimalLevelList Levels { get; set; }

    public static SerializedCategory FromLevelCategory(LevelCategory levelCategory,
        RequestContext context,
        GameDatabaseContext database,
        GameUser user,
        Token token,
        MatchService matchService,
        int skip = 0,
        int count = 20)
    {
        SerializedCategory category = new()
        {
            Name = levelCategory.Name,
            Description = levelCategory.Description,
            Url = "/searches/" + levelCategory.ApiRoute,
            Tag = "",
            IconHash = levelCategory.IconHash,
        };

        DatabaseList<GameLevel> categoryLevels = levelCategory.Fetch(context, skip, count, matchService, database, user, new LevelFilterSettings(context, token.TokenGame));
        
        IEnumerable<GameMinimalLevelResponse> levels = categoryLevels?.Items
            .Select(l => GameMinimalLevelResponse.FromOldWithExtraData(l, matchService)) ?? Array.Empty<GameMinimalLevelResponse>();

        category.Levels = new SerializedMinimalLevelList(levels, categoryLevels?.TotalItems ?? 0);

        return category;
    }
}