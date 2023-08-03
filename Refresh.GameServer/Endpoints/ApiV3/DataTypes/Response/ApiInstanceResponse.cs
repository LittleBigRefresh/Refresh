using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiInstanceResponse : IApiResponse
{
    public required string InstanceName { get; set; }
    public required string InstanceDescription { get; set; }
    public required string SoftwareName { get; set; }
    public required string SoftwareVersion { get; set; }
    public required string SoftwareType { get; set; }
    
    public required bool RegistrationEnabled { get; set; }
    public required AssetSafetyLevel MaximumAssetSafetyLevel { get; set; }
}