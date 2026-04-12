using Refresh.Core.Configuration;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiRolePermissionsResponse : IApiResponse
{
    public required ConfigAssetFlags BlockedAssetFlags { get; set; }
    public required bool ReadOnlyMode { get; set; }
    public required ApiTimedLevelLimitResponse? TimedLevelUploadLimits { get; set; }
    public required int UserFilesizeQuota { get; set; }

    public static ApiRolePermissionsResponse FromOld(RolePermissions old)
    {
        return new()
        {
            BlockedAssetFlags = old.BlockedAssetFlags,
            ReadOnlyMode = old.ReadOnlyMode,
            TimedLevelUploadLimits = old.TimedLevelUploadLimits.Enabled ? new ApiTimedLevelLimitResponse()
            {
                TimeSpanHours = old.TimedLevelUploadLimits.TimeSpanHours,
                LevelQuota = old.TimedLevelUploadLimits.LevelQuota,
            } : null,
            UserFilesizeQuota = old.UserFilesizeQuota,
        };
    }
}