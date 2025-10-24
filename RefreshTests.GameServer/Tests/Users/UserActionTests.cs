using Refresh.Common.Constants;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Users;

public class UserActionTests : GameServerTest
{
    [Test]
    public void RenamesUser()
    {
        using TestContext context = this.GetServer(false);
        GameUser? user = context.CreateUser("gamer1");
        
        Assert.That(user.Username, Is.EqualTo("gamer1"));
        
        context.Database.RenameUser(user, "gamer2");
        user = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(user, Is.Not.Null);
        
        Assert.That(user!.Username, Is.EqualTo("gamer2"));
    }

    [Test]
    public void UserDescriptionGetsTrimmed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedUpdateDataProfile request = new()
        {
            Description = new string('S', 600),
        };

        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        // Ensure the description was trimmed
        GameUser? updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Description.Length, Is.EqualTo(UgcLimits.DescriptionLimit));
    }
}