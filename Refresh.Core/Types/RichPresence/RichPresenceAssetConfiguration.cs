namespace Refresh.Core.Types.RichPresence;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RichPresenceAssetConfiguration
{
    public required bool UseApplicationAssets { get; set; }
    public required string? PodAsset { get; set; }
    public required string? MoonAsset { get; set; }
    public required string? RemoteMoonAsset { get; set; }
    public required string? DeveloperAsset { get; set; }
    public required string? DeveloperAdventureAsset { get; set; }
    public required string? DlcAsset { get; set; }
    public required string? FallbackAsset { get; set; }
}