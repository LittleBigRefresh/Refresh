using Refresh.GameServer.Configuration;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiInstanceResponse : IApiResponse
{
    /// <summary>
    /// An admin-configurable name for this instance.
    /// </summary>
    public required string InstanceName { get; set; }
    /// <summary>
    /// An admin-configurable description of this instance.
    /// </summary>
    public required string InstanceDescription { get; set; }
    /// <summary>
    /// The name of the software. We use 'Refresh'.
    /// </summary>
    public required string SoftwareName { get; set; }
    /// <summary>
    /// The version of the software. Can be SemVer, or simply a commit hash.
    /// </summary>
    public required string SoftwareVersion { get; set; }
    /// <summary>
    /// The build type of the software. For example we use either Debug or Release.
    /// </summary>
    public required string SoftwareType { get; set; }
    /// <summary>
    /// A URL pointing to the source code of the software.
    /// </summary>
    public required string SoftwareSourceUrl { get; set; }
    /// <summary>
    /// A short name describing the license of the server software.
    /// </summary>
    public required string SoftwareLicenseName { get; set; }
    /// <summary>
    /// A link to the server's license.
    /// </summary>
    public required string SoftwareLicenseUrl { get; set; }
    
    public required bool RegistrationEnabled { get; set; }
    public required ConfigAssetFlags BlockedAssetFlags { get; set; }
    public required ConfigAssetFlags BlockedAssetFlagsForTrustedUsers { get; set; }
    
    public required IEnumerable<ApiGameAnnouncementResponse> Announcements { get; set; }
    public required ApiRichPresenceConfigurationResponse RichPresenceConfiguration { get; set; }
    
    public required bool MaintenanceModeEnabled { get; set; }
    public required string? GrafanaDashboardUrl { get; set; }
    
    public required string WebsiteLogoUrl { get; set; }
    
    public required ApiContactInfoResponse ContactInfo { get; set; }
    
    public required ApiContestResponse? ActiveContest { get; set; }
}