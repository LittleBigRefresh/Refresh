using System.Xml.Serialization;
using Bunkum.HttpServer;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

#nullable disable

[XmlType("category")]
public class GameCategory
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
    public GameMinimalLevelList Levels { get; set; }

    public static GameCategory FromLevelCategory(LevelCategory levelCategory, RequestContext context, RealmDatabaseContext database, GameUser user, int skip = 0, int count = 20)
    {
        GameCategory category = new()
        {
            Name = levelCategory.Name,
            Description = levelCategory.Description,
            Url = "/searches/" + levelCategory.ApiRoute,
            Tag = "",
            IconHash = levelCategory.IconHash,
        };
        
        IEnumerable<GameMinimalLevel> levels = levelCategory.Fetch(context, skip, count, database, user)?
            .Select(GameMinimalLevel.FromGameLevel) ?? Array.Empty<GameMinimalLevel>();

        category.Levels = new GameMinimalLevelList(levels, 10);

        return category;
    }
}