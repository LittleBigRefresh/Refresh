using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Users;

public class RegistrationQueueTests : GameServerTest
{
    [Test]
    public void CreateAccountFromQueueDespiteWrongUsernameCasing()
    {
        using TestContext context = this.GetServer();
        const string wrongUsername = "spellingHobby21";
        const string correctUsername = "SpelliNgHobby21";

        // Add new registration to queue with misspelled name
        context.Database.AddRegistrationToQueue(wrongUsername, "spelling@hobby.real", "veri sekure");

        // Now get it using the actually correct username, and check its attributes
        QueuedRegistration? registration = context.Database.GetQueuedRegistrationByUsername(correctUsername);
        Assert.That(registration, Is.Not.Null);
        Assert.That(registration!.Username, Is.EqualTo(correctUsername));
        Assert.That(registration!.UsernameLower, Is.EqualTo(wrongUsername.ToLower()));

        // Now use it to create an account and check whether it's been properly created
        GameUser user = context.Database.CreateUserFromQueuedRegistration(registration, TokenPlatform.PS3);
        Assert.That(user.Username, Is.EqualTo(correctUsername));
        Assert.That(user.UsernameLower, Is.EqualTo(wrongUsername.ToLower()));
        Assert.That(context.Database.GetUserByUsername(correctUsername, true)?.Username, Is.EqualTo(correctUsername));
        Assert.That(context.Database.GetUserByUsername(wrongUsername, false)?.Username, Is.EqualTo(correctUsername));
        Assert.That(context.Database.GetUserByUsername(wrongUsername, false)?.UserId, Is.EqualTo(context.Database.GetUserByUsername(correctUsername, false)?.UserId));
        Assert.That(context.Database.GetUserByUsername(wrongUsername, true)?.Username, Is.Null);
    }
}