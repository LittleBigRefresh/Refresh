using Refresh.Common.Constants;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
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

    [Test]
    [TestCase("")]
    [TestCase("0")]
    public void CanResetOwnIcon(string newIcon)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        // Prepare by setting icon to something
        string fakeIcon = "mmmmm";
        context.Database.UpdateUserData(user, new ApiUpdateUserRequest()
        {
            IconHash = fakeIcon
        });
        GameUser? userPrepared = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userPrepared, Is.Not.Null);
        Assert.That(userPrepared!.IconHash, Is.EqualTo(fakeIcon));

        // Now try resetting
        SerializedUpdateDataPlanets request = new()
        {
            IconHash = newIcon
        };
        HttpResponseMessage response = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(response, Is.Not.Null);

        context.Database.Refresh();

        GameUser? userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated, Is.Not.Null);
        Assert.That(userUpdated!.IconHash, Is.EqualTo("0"));
    }
}