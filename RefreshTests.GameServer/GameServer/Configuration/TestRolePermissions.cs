using Refresh.Core.Configuration;
using Refresh.Database.Models.Assets;

namespace RefreshTests.GameServer.GameServer.Configuration;

public class TestRolePermissions : RolePermissions
{
    public TestRolePermissions() {}

    public TestEntityUploadRateLimitProperties TimedLevelUploadLimits = new()
    {
        Enabled = false,
        TimeSpanHours = 24,
        LevelQuota = 8,
    };
}