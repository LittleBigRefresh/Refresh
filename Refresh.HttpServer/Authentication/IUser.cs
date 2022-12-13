using Newtonsoft.Json;

namespace Refresh.HttpServer.Authentication;

#nullable disable

public interface IUser
{
    [JsonProperty("userId")]
    public ulong UserId { get; set; }
    [JsonProperty("username")]
    public string Username { get; set; }
}