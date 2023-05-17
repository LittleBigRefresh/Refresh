using Newtonsoft.Json;

namespace RefreshTests.GameServer.Extensions;

public static class HttpClientExtensions
{
    public static async Task<(TObject?, HttpResponseMessage)> GetJsonObjectAsync<TObject>(this HttpClient client, string url) where TObject : class
    {
        HttpResponseMessage message = await client.GetAsync(url);
        Stream stream = await message.Content.ReadAsStreamAsync();

        JsonSerializer serializer = new();
        using StreamReader streamReader = new(stream);
        await using JsonTextReader jsonReader = new(streamReader);

        TObject? obj = serializer.Deserialize<TObject>(jsonReader);
        return (obj, message);
    }
}