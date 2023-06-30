namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

/// <summary>
/// A response from the API containing information about a list and part of it's contents.
/// </summary>
[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiListResponse<TData> : ApiResponse<List<TData>> where TData : class
{
    public ApiListResponse(IEnumerable<TData> data, ApiListInformation info) : base(data.ToList())
    {
        this.ListInfo = info;
    }
    
    public ApiListResponse(IEnumerable<TData> data) : base(data.ToList())
    {
        ApiListInformation info = new()
        {
            EntryCount = this.Data!.Count,
            NextPageIndex = this.Data.Count + 1,
        };

        this.ListInfo = info;
    }

    public ApiListResponse(ApiError error) : base(error)
    {
        this.ListInfo = null;
    }
    
    [JsonProperty("listInfo")] public ApiListInformation? ListInfo { get; set; }
}