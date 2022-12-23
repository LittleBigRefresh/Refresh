using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Realms;

namespace Refresh.GameServer.Types.UserData; 

#nullable disable

public class UserPins : EmbeddedObject {
	[JsonProperty(PropertyName = "progress")]
	public IList<long> Progress { get; }
	[JsonProperty(PropertyName = "awards")]
	public IList<long> Awards { get; }
	[JsonProperty(PropertyName = "profile_pins")]
	public IList<long> ProfilePins { get; }
}
