using JetBrains.Annotations;
using Newtonsoft.Json;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes;

namespace RefreshTests.GameServer.Extensions;

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
    public static ApiResponse<TData>? GetData<TData>(this HttpClient client, string endpoint, bool ensureSuccessful = true, bool ensureFailure = false) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.GetAsync(endpoint).Result;
        if (ensureSuccessful)
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        else if (ensureFailure)
            Assert.That(response.StatusCode, Is.Not.EqualTo(OK));
        return ReadData<TData>(response.Content);
    }
    
    [Pure]
    public static ApiListResponse<TData>? GetList<TData>(this HttpClient client, string endpoint, bool ensureSuccessful = true, bool ensureFailure = false) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.GetAsync(endpoint).Result;
        if (ensureSuccessful)
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        else if (ensureFailure)
            Assert.That(response.StatusCode, Is.Not.EqualTo(OK));
        return ReadList<TData>(response.Content);
    }
    
    public static ApiResponse<TData>? PostData<TData>(this HttpClient client, string endpoint, object data, bool ensureSuccessful = true, bool ensureFailure = false) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.PostAsync(endpoint, new StringContent(data.AsJson())).Result;
        if (ensureSuccessful)
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        else if (ensureFailure)
            Assert.That(response.StatusCode, Is.Not.EqualTo(OK));
        return ReadData<TData>(response.Content);
    }
    
    public static ApiResponse<TData>? PatchData<TData>(this HttpClient client, string endpoint, object data, bool ensureSuccessful = true, bool ensureFailure = false) where TData : class, IApiResponse
    {
        HttpResponseMessage response = client.PatchAsync(endpoint, new StringContent(data.AsJson())).Result;
        if (ensureSuccessful)
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        else if (ensureFailure)
            Assert.That(response.StatusCode, Is.Not.EqualTo(OK));
        return ReadData<TData>(response.Content);
    }
}