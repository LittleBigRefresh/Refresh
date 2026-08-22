using Refresh.Core.Configuration;

namespace RefreshTests.GameServer.GameServer.Configuration;

public class TestEndpointRateLimitConfig : EndpointRateLimitConfig
{
    public void TestMigration()
    {
        this.Migrate(this.Version, this);
    }
}