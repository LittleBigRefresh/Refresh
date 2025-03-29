using System.Xml.Serialization;
using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Users;

#nullable disable

public class SerializedUserCategory : SerializedCategory
{
    [XmlElement("results")] public SerializedUserList Users { get; set; }

    public static SerializedUserCategory FromUserCategory(GameCategory category)
    {
        SerializedUserCategory serializedCategory = new()
        {
            Name = category.Name,
            Description = category.Description,
            Url = "/searches/users/" + category.ApiRoute,
            Tag = category.ApiRoute,
            Types = 
            [
                "user",
            ],
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

        LevelFilterSettings filterSettings = new(context, dataContext.Token!.TokenGame);
        DatabaseList<GameUser> categoryUsers = userCategory.Fetch(context, skip, count, dataContext, filterSettings, dataContext.User);
        
        IEnumerable<GameUserResponse> users = categoryUsers?.Items
            .Select(l => GameUserResponse.FromOld(l, dataContext)) ?? [];

        serializedUserCategory.Users = new SerializedUserList
        {
            Items = users.ToList(),
        };

        return serializedUserCategory;
    }
}