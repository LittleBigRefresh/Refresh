using Newtonsoft.Json;

namespace Refresh.GameServer.Database.Types; 

#pragma warning disable CS8618
public class UpdateUserPins
{
    [JsonProperty(PropertyName = "progress")]
    public long[] Progress { get; set; }
    [JsonProperty(PropertyName = "awards")]
    public long[] Awards { get; set; }
    [JsonProperty(PropertyName = "profile_pins")]
    public long[] ProfilePins { get; set; }
}

