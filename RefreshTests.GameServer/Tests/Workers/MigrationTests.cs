using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Workers;
using Refresh.Workers.State;

namespace RefreshTests.GameServer.Tests.Workers;

public class MigrationTests : GameServerTest
{
    [Test]
    public void MigrationJobWorks()
    {
        using TestContext context = this.GetServer();
        TestMigrationJob job = new();
        GameUser user = context.CreateUser();

        for (int i = 0; i < 100; i++)
        {
            context.CreateLevel(user);
        }

        IEnumerable<GameLevel> allLevels = context.Database.GetNewestLevels(100, 0, null, new(TokenGame.Website)).Items;
        Assert.That(allLevels.All(l => l.Title == "Level"), Is.True);

        job.JobState = new MigrationJobState();
        job.ExecuteJob(context.GetWorkContext());
        
        allLevels = context.Database.GetNewestLevels(100, 0, null, new(TokenGame.Website)).Items;
        Assert.That(allLevels.All(l => l.Title == "Level test"), Is.True);
    }

    [Test]
    public void DeletingEntitiesDuringMigrationJobDoesNotBreakPagination()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        // 3 cycles (6 levels per cycle)
        for (int i = 0; i < 18; i++)
        {
            context.CreateLevel(user);
        }
        context.Database.Refresh();
        Assert.That(context.Database.GetTotalLevelCount(), Is.EqualTo(18));

        // Prepare migration
        TestMigrationJobDeletingLevels job = new();
        context.Database.UpdateOrCreateJobState(typeof(TestMigrationJobDeletingLevels).Name, new MigrationJobState()
        {
            Total = 18
        }, WorkerClass.Refresh);
        context.Database.Refresh();

        object? stateObject = context.Database.GetJobState(typeof(TestMigrationJobDeletingLevels).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);
        job.JobState = stateObject!;
        
        // Migrate - First cycle
        job.ExecuteJob(context.GetWorkContext());
        context.Database.Refresh();

        // Ensure only 3 levels were actually migrated, and 3 levels were deleted
        Assert.That(context.Database.GetTotalLevelCount(), Is.EqualTo(15));
        Assert.That(context.Database.GetNewestLevels(20, 0, null, new(TokenGame.Website)).Items.Count(l => l.Title.EndsWith(" test")), Is.EqualTo(3));

        stateObject = context.Database.GetJobState(typeof(TestMigrationJobDeletingLevels).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        MigrationJobState jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Processed, Is.EqualTo(3));
        Assert.That(jobState.Total, Is.EqualTo(15));
        Assert.That(jobState.Complete, Is.False);

        // Migrate - Second cycle
        job.ExecuteJob(context.GetWorkContext());
        context.Database.Refresh();

        // Ensure 3 more levels were deleted, and 3 more levels were migrated
        Assert.That(context.Database.GetTotalLevelCount(), Is.EqualTo(12));
        Assert.That(context.Database.GetNewestLevels(20, 0, null, new(TokenGame.Website)).Items.Count(l => l.Title.EndsWith(" test")), Is.EqualTo(6));

        stateObject = context.Database.GetJobState(typeof(TestMigrationJobDeletingLevels).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Processed, Is.EqualTo(6));
        Assert.That(jobState.Total, Is.EqualTo(12));
        Assert.That(jobState.Complete, Is.False);

        // Migrate - Last cycle
        job.ExecuteJob(context.GetWorkContext());
        context.Database.Refresh();

        // Ensure 3 more levels were deleted, and 3 more levels were migrated
        Assert.That(context.Database.GetTotalLevelCount(), Is.EqualTo(9));
        Assert.That(context.Database.GetNewestLevels(20, 0, null, new(TokenGame.Website)).Items.Count(l => l.Title.EndsWith(" test")), Is.EqualTo(9));

        stateObject = context.Database.GetJobState(typeof(TestMigrationJobDeletingLevels).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Processed, Is.EqualTo(9));
        Assert.That(jobState.Total, Is.EqualTo(9));
        Assert.That(jobState.Complete, Is.True);
    }
}