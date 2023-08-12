using Refresh.GameServer.Endpoints.ApiV3.DataTypes;

namespace Refresh.GameServer.Types.RichPresence;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RichPresenceAssetConfiguration : IApiResponse
{
    public string? PodAsset { get; set; }
    public string? MoonAsset { get; set; }
    public string? RemoteMoonAsset { get; set; }
    public string? DeveloperAsset { get; set; }
    public string? DeveloperAdventureAsset { get; set; }
    public string? DlcAsset { get; set; }
    public string? FallbackAsset { get; set; }
}