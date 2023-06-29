using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes;

/// <summary>
/// A response from the API.
/// </summary>
[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiResponse<TData> where TData : class
{
    public ApiResponse(TData data)
    {
        this.Success = true;
        this.Data = data;
        this.Error = null;
    }

    public ApiResponse(ApiError error)
    {
        this.Success = false;
        this.Data = null;
        this.Error = error;
    }

    public static implicit operator ApiResponse<TData>(TData? data)
    {
        if (data == null) return new ApiResponse<TData>(new ApiError("Data was null, maybe internal validation failed?"));
        return new ApiResponse<TData>(data);
    }
    
    [JsonProperty("success")] public bool Success { get; private init; }
    [JsonProperty("data")] public TData? Data { get; private init; }
    [JsonProperty("error")] public ApiError? Error { get; private init; }
}