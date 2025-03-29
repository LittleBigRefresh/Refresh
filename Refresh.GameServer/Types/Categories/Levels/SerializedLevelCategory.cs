using System.Xml.Serialization;
using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;

namespace Refresh.GameServer.Types.Categories.Levels;

#nullable disable

public class SerializedLevelCategory : SerializedCategory
{
    [XmlElement("results")] public SerializedMinimalLevelList Levels { get; set; }

    public static SerializedLevelCategory FromLevelCategory(GameLevelCategory category)
    {
        SerializedLevelCategory serializedCategory = new()
        {
            Name = category.Name,
            Description = category.Description,
            Url = "/searches/levels/" + category.ApiRoute,
            Tag = category.ApiRoute,
            Types = 
            [
                "slot",
                "adventure",
            ],
            IconHash = category.IconHash,
        };

        return serializedCategory;
    }

    public static SerializedLevelCategory FromLevelCategory(GameLevelCategory levelCategory,
        RequestContext context,
        DataContext dataContext,
        int skip = 0,
        int count = 20)
    {
        SerializedLevelCategory serializedLevelCategory = FromLevelCategory(levelCategory);

        LevelFilterSettings filterSettings = new(context, dataContext.Token!.TokenGame);
        DatabaseList<GameLevel> categoryLevels = levelCategory.Fetch(context, skip, count, dataContext, filterSettings, dataContext.User);
        
        IEnumerable<GameMinimalLevelResponse> levels = categoryLevels?.Items
            .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext)) ?? [];

        serializedLevelCategory.Levels = new SerializedMinimalLevelList(levels, categoryLevels?.TotalItems ?? 0, skip + count);

        return serializedLevelCategory;
    }
}