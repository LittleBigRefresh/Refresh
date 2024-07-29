using System.Xml.Serialization;
using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
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

    public static SerializedCategory FromLevelCategory(LevelCategory levelCategory)
    {
        SerializedCategory category = new()
        {
            Name = levelCategory.Name,
            Description = levelCategory.Description,
            Url = "/searches/" + levelCategory.ApiRoute,
            Tag = levelCategory.ApiRoute,
            IconHash = levelCategory.IconHash,
        };

        return category;
    }

    public static SerializedCategory FromLevelCategory(LevelCategory levelCategory,
        RequestContext context,
        DataContext dataContext,
        int skip = 0,
        int count = 20)
    {
        SerializedCategory category = FromLevelCategory(levelCategory);

        LevelFilterSettings filterSettings = new(context, dataContext.Token!.TokenGame);
        DatabaseList<GameLevel> categoryLevels = levelCategory.Fetch(context, skip, count, dataContext, filterSettings, dataContext.User);
        
        IEnumerable<GameMinimalLevelResponse> levels = categoryLevels?.Items
            .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext)) ?? Array.Empty<GameMinimalLevelResponse>();

        category.Levels = new SerializedMinimalLevelList(levels, categoryLevels?.TotalItems ?? 0, skip + count);

        return category;
    }
}