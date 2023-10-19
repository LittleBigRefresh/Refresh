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
    
    public static ApiResponse<TData>? GetData<TData>(this HttpClient client, string endpoint) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.GetAsync(endpoint).Result;
        return ReadData<TData>(response.Content);
    }
    
    public static ApiResponse<TData>? PostData<TData>(this HttpClient client, string endpoint, HttpContent data) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.PostAsync(endpoint, data).Result;
        return ReadData<TData>(response.Content);
    }
}