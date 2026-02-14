using System.Xml.Serialization;
using Bunkum.Core;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Data;
using Refresh.Database.Query;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Types.Categories;

#nullable disable

[XmlType("category")]
public class SerializedCategory
{
    [XmlElement("name")] public string Name { get; set; }
    [XmlElement("description")] public string Description { get; set; }
    [XmlElement("url")] public string Url { get; set; }
    [XmlElement("tag")] public string Tag { get; set; }
    [XmlElement("icon")] public string IconHash { get; set; }
    [XmlElement("results")] public SerializedCategoryResultsList Results { get; set; }

    public static SerializedCategory FromCategory(GameCategory category)
    {
        string urlSubRoute = category.PrimaryResultType switch
        {
            ResultType.Level => "levels",
            ResultType.User => "users",
            ResultType.Playlist => "playlists",
            _ => throw new ArgumentOutOfRangeException(nameof(category.PrimaryResultType), category.PrimaryResultType, "Unknown category type"),
        };

        SerializedCategory serializedCategory = new()
        {
            Name = category.Name,
            Description = category.Description,
            Url = $"/searches/{urlSubRoute}/{category.ApiRoute}",
            Tag = category.ApiRoute,
            IconHash = category.IconHash,
        };

        return serializedCategory;
    }

    #nullable restore

    public static SerializedCategory FromCategory(GameCategory category,
        RequestContext context,
        DataContext dataContext,
        int skip = 0,
        int count = 20)
    {
        SerializedCategory serializedCategory = FromCategory(category);

        LevelFilterSettings filterSettings = LevelFilterSettings.FromGameRequest(context, dataContext.Game, true);
        DatabaseResultList? results = category.Fetch(context, skip, count, dataContext, filterSettings, dataContext.User);

        if (results != null)
        {
            serializedCategory.Results = new()
            {
                Levels = GameMinimalLevelResponse.FromOldList(results.Levels?.Items.ToArray() ?? [], dataContext).ToList(),
                Users = GameUserResponse.FromOldList(results.Users?.Items.ToArray() ?? [], dataContext).ToList(),
                Total = results.TotalItems,
                NextPageStart = -1
            };
        }

        return serializedCategory;
    }
}