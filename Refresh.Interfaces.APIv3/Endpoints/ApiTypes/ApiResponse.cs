using System.Net;
using Bunkum.Core.Responses;

namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes;

/// <summary>
/// A response from the API.
/// </summary>
[JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiResponse<TData> : IHasResponseCode where TData : class
{
    /// <summary>
    /// Empty constructor for serialization. Do not use.
    /// </summary>
    [Obsolete("Empty constructor for serialization.", true)]
    public ApiResponse()
    {
        
    }
    
    public ApiResponse(TData data, HttpStatusCode statusCode = OK)
    {
        this.Success = true;
        this.Data = data;
        this.Error = null;

        this.StatusCode = statusCode;
    }

    public ApiResponse(ApiError error)
    {
        this.Success = false;
        this.Data = null;
        this.Error = error;

        this.StatusCode = error.StatusCode;
    }

    public static implicit operator ApiResponse<TData>(TData? data)
    {
        if (data == null) return new ApiResponse<TData>(new ApiError("Data was null, maybe internal validation failed?"));
        return new ApiResponse<TData>(data);
    }
    
    public static implicit operator ApiResponse<TData>(ApiError error)
    {
        return new ApiResponse<TData>(error);
    }
    
    public HttpStatusCode StatusCode { get; private set; }
    
    [JsonProperty("success")] public bool Success { get; private init; }
    [JsonProperty("data")] public TData? Data { get; private init; }
    [JsonProperty("error")] public ApiError? Error { get; private init; }
}