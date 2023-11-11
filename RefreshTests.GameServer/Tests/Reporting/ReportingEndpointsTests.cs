using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Reporting;

public class ReportingEndpointsTests : GameServerTest
{
    [Test]
    public void UploadReport()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
    }
}