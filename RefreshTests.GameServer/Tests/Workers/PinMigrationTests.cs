using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Workers;
using Refresh.Interfaces.Workers.Migrations;
using Refresh.Workers.State;

namespace RefreshTests.GameServer.Tests.Workers;

public class PinMigrationTests : GameServerTest
{
    [Test]
    public void WebsitePinMigrationDoesNotBreakPagination()
    {
        using TestContext context = this.GetServer();

        for (int i = 0; i < 50; i++)
        {
            GameUser user = context.CreateUser();
            context.Database.AddPinProgress(new()
            {
                PinId = (long)ServerPins.SignIntoWebsite,
                Progress = i + 1,
                Publisher = user,
                FirstPublished = new(),
                LastUpdated = new(),
                IsBeta = false,
                Platform = TokenPlatform.RPCS3,
            }, false);

            context.Database.AddPinProgress(new()
            {
                PinId = (long)ServerPins.SignIntoWebsite,
                Progress = i + 1,
                Publisher = user,
                FirstPublished = new(),
                LastUpdated = new(),
                IsBeta = true,
                Platform = TokenPlatform.RPCS3,
            }, false);
        }
        context.Database.SaveChanges();
        Assert.That(context.Database.GetTotalPinProgresses(), Is.EqualTo(100));

        // Prepare migration
        CorrectWebsitePinProgressPlatformMigration job = new();
        context.Database.UpdateOrCreateJobState(typeof(CorrectWebsitePinProgressPlatformMigration).Name, new MigrationJobState()
        {
            Total = 100 // Since we already have to manually create the state
        }, WorkerClass.Refresh);
        context.Database.Refresh();

        object? stateObject = context.Database.GetJobState(typeof(CorrectWebsitePinProgressPlatformMigration).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);
        job.JobState = stateObject!;
        
        // Migrate
        job.ExecuteJob(context.GetWorkContext());
        context.Database.Refresh();

        stateObject = context.Database.GetJobState(typeof(CorrectWebsitePinProgressPlatformMigration).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        // While we're not actually testing pagination here (batch count can't be edited + don't want to create over 1000 users here, would take too long),
        // it should be enough to simply ensure that both the total and processed counts are adjusted to be less than 100 (as 50 pins were deleted)
        MigrationJobState jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Processed, Is.EqualTo(50));
        Assert.That(jobState.Total, Is.EqualTo(50));
        Assert.That(jobState.Complete, Is.True);

        Assert.That(context.Database.GetTotalPinProgresses(), Is.EqualTo(50));
    }
}