using Refresh.GameServer.Services;

namespace RefreshTests.GameServer.Tests.Matching;

public class MatchingTests : GameServerTest
{
    [Test]
    public void MatchesTwoPlayersTogether()
    {
        using TestContext context = this.GetServer(false);
        MatchService service = new(Logger);
        service.Initialize();
    }
}