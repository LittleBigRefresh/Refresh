using System.Xml.Serialization;
using Bunkum.Core;
using Refresh.Core.Types.Categories.Users;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Types.Categories;

#nullable disable

public class SerializedUserCategory : SerializedCategory
{
    [XmlElement("results")] public SerializedUserList Users { get; set; }

    public static SerializedUserCategory FromUserCategory(GameUserCategory category)
    {
        SerializedUserCategory serializedCategory = new()
        {
            Name = category.Name,
            Description = category.Description,
            Url = "/searches/users/" + category.ApiRoute,
            Tag = category.ApiRoute,
            IconHash = category.IconHash,
        };

        return serializedCategory;
    }

    public static SerializedUserCategory FromUserCategory(GameUserCategory userCategory,
        RequestContext context,
        DataContext dataContext,
        int skip = 0,
        int count = 20)
    {
        SerializedUserCategory serializedUserCategory = FromUserCategory(userCategory);
        DatabaseList<GameUser> categoryLevels = userCategory.Fetch(context, skip, count, dataContext, dataContext.User);
        
        IEnumerable<GameUserResponse> users = categoryLevels?.Items.ToArray()
            .Select(l => GameUserResponse.FromOld(l, dataContext)) ?? [];

        serializedUserCategory.Users = new SerializedUserList() { Users = users.ToList() };

        return serializedUserCategory;
    }
}