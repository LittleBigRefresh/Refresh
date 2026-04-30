using Refresh.Core.Configuration;

namespace RefreshTests.GameServer.GameServer.Configuration;

public class TestEntityUploadRateLimitProperties : EntityUploadRateLimitProperties
{
    public TestEntityUploadRateLimitProperties() {}

    public int LevelQuota { get; set; }
}