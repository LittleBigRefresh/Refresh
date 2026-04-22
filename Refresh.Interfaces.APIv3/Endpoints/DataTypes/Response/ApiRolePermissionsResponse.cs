using Refresh.Core.Configuration;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiRolePermissionsResponse : IApiResponse
{
    public required ConfigAssetFlags BlockedAssetFlags { get; set; }
    public required bool ReadOnlyMode { get; set; }
    public required ApiEntityRateLimitResponse? LevelUploadRateLimit { get; set; }
    public required ApiEntityRateLimitResponse? PhotoUploadRateLimit { get; set; }
    public required ApiEntityRateLimitResponse? PlaylistUploadRateLimit { get; set; }
    public required int UserFilesizeQuota { get; set; }

    public static ApiRolePermissionsResponse FromOld(RolePermissions old)
    {
        return new()
        {
            BlockedAssetFlags = old.BlockedAssetFlags,
            ReadOnlyMode = old.ReadOnlyMode,
            LevelUploadRateLimit = ApiEntityRateLimitResponse.FromOld(old.LevelUploadRateLimit),
            PhotoUploadRateLimit = ApiEntityRateLimitResponse.FromOld(old.PhotoUploadRateLimit),
            PlaylistUploadRateLimit = ApiEntityRateLimitResponse.FromOld(old.PlaylistUploadRateLimit),
            UserFilesizeQuota = old.UserFilesizeQuota,
        };
    }
}