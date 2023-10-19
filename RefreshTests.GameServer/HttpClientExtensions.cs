using JetBrains.Annotations;
using Newtonsoft.Json;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;

namespace RefreshTests.GameServer;

public static class HttpClientExtensions
{
    private static ApiResponse<TData>? ReadData<TData>(HttpContent content) where TData : class, IApiResponse
    {
        return JsonConvert.DeserializeObject<ApiResponse<TData>>(content.ReadAsStringAsync().Result);
    }
    
    private static ApiListResponse<TData>? ReadList<TData>(HttpContent content) where TData : class, IApiResponse
    {
        return JsonConvert.DeserializeObject<ApiListResponse<TData>>(content.ReadAsStringAsync().Result);
    }
    
    [Pure]
    public static ApiResponse<TData>? GetData<TData>(this HttpClient client, string endpoint) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.GetAsync(endpoint).Result;
        return ReadData<TData>(response.Content);
    }
    
    [Pure]
    public static ApiListResponse<TData>? GetList<TData>(this HttpClient client, string endpoint) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.GetAsync(endpoint).Result;
        return ReadList<TData>(response.Content);
    }
    
    public static ApiResponse<TData>? PostData<TData>(this HttpClient client, string endpoint, object data) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.PostAsync(endpoint, new StringContent(data.AsJson())).Result;
        return ReadData<TData>(response.Content);
    }
    
    public static ApiResponse<TData>? PatchData<TData>(this HttpClient client, string endpoint, object data) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.PatchAsync(endpoint, new StringContent(data.AsJson())).Result;
        return ReadData<TData>(response.Content);
    }
}