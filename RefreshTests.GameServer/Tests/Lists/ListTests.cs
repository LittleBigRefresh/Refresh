using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Lists;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Lists;

public class ListTests : GameServerTest
{
    [Test]
    public async Task LevelListPaginatesCorrectly()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        const int pages = 5;
        const int pageSize = 10;
        
        for (int i = 0; i < pageSize * pages; i++)
        {
            context.CreateLevel(user, i.ToString());
        }

        int page = 0;
        while (true)
        {
            HttpResponseMessage message = await client.GetAsync($"/lbp/slots/newest?pageStart={(pageSize * page) + 1}&pageSize={pageSize}");
            SerializedMinimalLevelList levelList = message.Content.ReadAsXML<SerializedMinimalLevelList>();

            if (pageSize * page >= levelList.Total) break;
            Assert.Multiple(() =>
            {
                Assert.That(levelList.Items[0].LevelId, Is.EqualTo((pageSize * page) + 1), $"first item is invalid on page {page + 1}");
                Assert.That(levelList.Items[9].LevelId, Is.EqualTo((pageSize * page) + 10), $"last item is invalid on page {page + 1}");
            });

            page++;
        }
    }

    [Test]
    public async Task LevelListReturnsCorrectHintStart()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        for (int i = 0; i < 20; i++)
        {
            context.CreateLevel(user, i.ToString());
        }
        
        HttpResponseMessage message = await client.GetAsync("/lbp/slots/newest?pageStart=11&pageSize=10");
        string response = await message.Content.ReadAsStringAsync();
        
        Assert.That(response, Contains.Substring("hint_start=\"21\""));
    }
}