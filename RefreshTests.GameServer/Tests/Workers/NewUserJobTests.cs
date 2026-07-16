using Refresh.Database.Models.Users;
using Refresh.Interfaces.Workers.Repeating;
using Refresh.Workers;

namespace RefreshTests.GameServer.Tests.Workers;

public class NewUserJobTests : GameServerTest
{
    [Test]
    [TestCase(120, GameUserRole.User)] // waiting exactly 2 hours is just enough for promotion
    [TestCase(130, GameUserRole.User)] // waiting 2 hours and 10 minutes is more than enough
    [TestCase(119, GameUserRole.NewUser)] // waiting 1 hour and 59 minutes is not enough, so stay as NewUser
    [TestCase(60, GameUserRole.NewUser)] // waiting just 1 hour is totally not enough
    public void NewUsersGetPromotedIfOldEnough(long fastForwardMinutes, GameUserRole resultingRole)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        Assert.That(user.Role, Is.EqualTo(GameUserRole.NewUser));
        
        // Prepare
        WorkContext workContext = new()
        {
            Database = context.Database,
            DataStore = context.GetDataStore(),
            Logger = context.Server.Value.Logger,
            TimeProvider = context.Time,
        };
        NewUserJob job = new(2); // Set required age to 2 hours
        
        // Ensure job doesn't promote the user immediately
        job.ExecuteJob(workContext);
        context.Database.Refresh();
        GameUser? updatedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.Role, Is.EqualTo(GameUserRole.NewUser));

        // skip forward 2 hours and try again
        context.Time.TimestampMilliseconds += 1000 * 60 * fastForwardMinutes;
        job.ExecuteJob(workContext);
        context.Database.Refresh();
        
        // Ensure job has promoted the user this time
        updatedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.Role, Is.EqualTo(resultingRole));
    }
}