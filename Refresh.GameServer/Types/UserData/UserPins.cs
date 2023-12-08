using Microsoft.EntityFrameworkCore;
using Realms;

namespace Refresh.GameServer.Types.UserData; 

#nullable disable

[Keyless] // TODO: AGONY
public partial class UserPins : IEmbeddedObject
{ // TODO: Rename to GamePins
	[JsonProperty(PropertyName = "progress")]
	public IList<long> Progress { get; }
	[JsonProperty(PropertyName = "awards")]
	public IList<long> Awards { get; }
	[JsonProperty(PropertyName = "profile_pins")]
	public IList<long> ProfilePins { get; }
}
