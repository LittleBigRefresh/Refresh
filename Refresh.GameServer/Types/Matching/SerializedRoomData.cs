using System.Diagnostics.CodeAnalysis;

namespace Refresh.GameServer.Types.Matching;

[SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names")]
public class SerializedRoomData
{
    [JsonProperty("Player")]
    public string? Player { get; set; }
    
    [JsonProperty("Players")]
    public List<string>? Players { get; set; }

    [JsonProperty("Reservations")]
    public List<int>? Reservations { get; set; }

    [JsonProperty("NAT")]
    public List<NatType>? NatType { get; set; }

    // ReSharper disable InconsistentNaming
    // LBP has two of the same thing sometimes, have both properties to handle both cases
    [JsonProperty("Slot")]
    private List<int>? _Slot { get; set; }
    
    [JsonProperty("Slots")]
    private List<int>? _Slots { get; set; }
    // ReSharper restore InconsistentNaming

    [JsonIgnore]
    public List<int>? Slots => this._Slot ?? this._Slots;
    
    [JsonProperty("RoomState")]
    public RoomState? RoomState { get; set; }

    [JsonProperty("HostMood")]
    public byte? HostMood { get; set; }
    
    [JsonProperty("Mood")]
    public byte? Mood { get; set; }

    [JsonProperty("PassedNoJoinPoint")]
    public bool? PassedNoJoinPoint { get; set; }

    [JsonProperty("Location")]
    public List<string>? Locations { get; set; }

    [JsonProperty("Language")]
    public byte? Language { get; set; }

    [JsonProperty("BuildVersion")]
    public int? BuildVersion { get; set; }

    [JsonProperty("Search")]
    public string? Search { get; set; }
}