using System.Xml.Serialization;
using Bunkum.HttpServer;
using Refresh.GameServer.Database;
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

    public static SerializedCategory FromLevelCategory(LevelCategory levelCategory, RequestContext context, GameDatabaseContext database, GameUser user, int skip = 0, int count = 20)
    {
        SerializedCategory category = new()
        {
            Name = levelCategory.Name,
            Description = levelCategory.Description,
            Url = "/searches/" + levelCategory.ApiRoute,
            Tag = "",
            IconHash = levelCategory.IconHash,
        };

        DatabaseList<GameLevel> categoryLevels = levelCategory.Fetch(context, skip, count, database, user);
        
        IEnumerable<GameMinimalLevel> levels = categoryLevels?.Items
            .Select(GameMinimalLevel.FromGameLevel) ?? Array.Empty<GameMinimalLevel>();

        category.Levels = new SerializedMinimalLevelList(levels, categoryLevels?.TotalItems ?? 0);

        return category;
    }
}