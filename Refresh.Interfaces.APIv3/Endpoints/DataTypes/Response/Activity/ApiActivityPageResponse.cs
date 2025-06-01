using Refresh.Core.Types.Data;
using Refresh.Database.Models.Activity;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users.Photos;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Activity;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiActivityPageResponse : IApiResponse, IDataConvertableFrom<ApiActivityPageResponse, DatabaseActivityPage>
{
    public required IEnumerable<ApiEventResponse> Events { get; set; }
    public required IEnumerable<ApiActivityGroupResponse> Groups { get; set; }
    public required IEnumerable<ApiGameUserResponse> Users { get; set; }
    public required IEnumerable<ApiGameLevelResponse> Levels { get; set; }
    public required IEnumerable<ApiGameScoreResponse> Scores { get; set; }
    public required IEnumerable<ApiGamePhotoResponse> Photos { get; set; }

    public static ApiActivityPageResponse Empty => new()
    {
        Events = [],
        Groups = [],
        Levels = [],
        Photos = [],
        Scores = [],
        Users = [],
    };
    
    public static ApiActivityPageResponse? FromOld(DatabaseActivityPage? old, DataContext dataContext)
    {
        if (old == null) return null;

        List<Event> events = [];
        foreach (DatabaseActivityGroup group in old.EventGroups)
        {
            events.AddRange(group.Events);
            foreach (DatabaseActivityGroup subGroup in group.Children)
            {
                events.AddRange(subGroup.Events);
            }
        }

        return new ApiActivityPageResponse
        {
            Events = ApiEventResponse.FromOldList(events, dataContext),
            Groups = ApiActivityGroupResponse.FromOldList(old.EventGroups, dataContext),
            Users = ApiGameUserResponse.FromOldList(old.Users, dataContext),
            Levels = ApiGameLevelResponse.FromOldList(old.Levels, dataContext),
            Scores = ApiGameScoreResponse.FromOldList(old.Scores, dataContext),
            Photos = ApiGamePhotoResponse.FromOldList(old.Photos, dataContext),
        };
    }

    public static IEnumerable<ApiActivityPageResponse> FromOldList(IEnumerable<DatabaseActivityPage> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}