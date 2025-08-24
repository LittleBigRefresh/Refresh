using System.Xml.Serialization;
using Bunkum.Core;
using Refresh.Core.Types.Categories.Levels;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Query;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Types.Categories;

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

        LevelFilterSettings filterSettings = LevelFilterSettings.FromGameRequest(context, dataContext.Token!.TokenGame, true);
        DatabaseList<GameLevel> categoryLevels = levelCategory.Fetch(context, skip, count, dataContext, filterSettings, dataContext.User);
        
        IEnumerable<GameMinimalLevelResponse> levels = categoryLevels?.Items.ToArrayIfPostgres()
            .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext)) ?? [];

        serializedLevelCategory.Levels = new SerializedMinimalLevelList(levels, categoryLevels?.TotalItems ?? 0, skip + count);

        return serializedLevelCategory;
    }
}