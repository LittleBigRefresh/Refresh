using Refresh.Core.Types.Data;
using Refresh.Database.Models.Activity;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Activity;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiActivityGroupResponse : IApiResponse, IDataConvertableFrom<ApiActivityGroupResponse, DatabaseActivityGroup>
{
    public required string Type { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? UserId { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? LevelId { get; set; }
    
    public required DateTimeOffset Timestamp { get; set; }
    public required IEnumerable<ApiEventResponse> Events { get; set; }
    public required IEnumerable<ApiActivityGroupResponse> Children { get; set; }
    
    public static ApiActivityGroupResponse? FromOld(DatabaseActivityGroup? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        ApiActivityGroupResponse group = new()
        {
            Type = old.GroupType,
            Timestamp = old.Timestamp,
            Events = ApiEventResponse.FromOldList(old.Events, dataContext),
            Children = FromOldList(old.Children, dataContext),
        };

        if (old is DatabaseActivityLevelGroup levelGroup)
        {
            group.LevelId = levelGroup.Level.LevelId;
        }
        else if (old is DatabaseActivityUserGroup userGroup)
        {
            group.UserId = userGroup.User.UserId.ToString();
        }

        return group;
    }

    public static IEnumerable<ApiActivityGroupResponse> FromOldList(IEnumerable<DatabaseActivityGroup> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}