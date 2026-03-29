using Bunkum.Core.Storage;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Workers;
using Refresh.Workers;
using Refresh.Workers.State;

namespace RefreshTests.GameServer.Tests.Workers;

public class JobStateTests : GameServerTest
{
    [Test]
    public void RemovesJobStateIfJobDoesntExist()
    {
        using TestContext context = this.GetServer();
        IDataStore dataStore = context.GetDataStore();
        WorkerManager manager = new(Logger, dataStore, context.DatabaseProvider);

        context.Database.UpdateOrCreateJobState(typeof(TestMigrationJob).Name, new MigrationJobState(), WorkerClass.Refresh);
        Assert.That(context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Refresh), Is.Not.Null);

        manager.RemoveUnusedJobStates();
        manager.RunWorkCycle();
        context.Database.Refresh();

        object? stateObject = context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Null);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DoesNotRemoveJobStateIfJobExists(bool uploadLevel)
    {
        using TestContext context = this.GetServer();
        IDataStore dataStore = context.GetDataStore();
        WorkerManager manager = new(Logger, dataStore, context.DatabaseProvider);
        TestMigrationJob job = new();
        manager.AddJob(job);

        context.Database.UpdateOrCreateJobState(typeof(TestMigrationJob).Name, new MigrationJobState()
        {
            Total = uploadLevel ? 1 : 0, // Since we're creating the state manually, instead of having WorkerManager do it
        }, WorkerClass.Refresh);
        Assert.That(context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Refresh), Is.Not.Null);

        if (uploadLevel)
        {
            context.CreateLevel(context.CreateUser());
            Assert.That(context.Database.GetTotalLevelCount(), Is.EqualTo(1));
            
        }
        manager.RemoveUnusedJobStates();
        manager.RunWorkCycle();
        context.Database.Refresh();

        object? stateObject = context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        MigrationJobState jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Processed, Is.EqualTo(uploadLevel ? 1 : 0));
        Assert.That(jobState.Total, Is.EqualTo(uploadLevel ? 1 : 0));
        Assert.That(jobState.Complete, Is.True);
    }

    [Test]
    public void DoesNotRemoveJobStateIfNotRefreshClass()
    {
        using TestContext context = this.GetServer();
        IDataStore dataStore = context.GetDataStore();
        WorkerManager manager = new(Logger, dataStore, context.DatabaseProvider);
        
        context.Database.UpdateOrCreateJobState(typeof(TestMigrationJob).Name, new MigrationJobState(), WorkerClass.Craftworld);
        Assert.That(context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Craftworld), Is.Not.Null);

        manager.RemoveUnusedJobStates();
        manager.RunWorkCycle();
        context.Database.Refresh();

        object? stateObject = context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Craftworld);
        Assert.That(stateObject, Is.Not.Null);

        MigrationJobState jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Complete, Is.True);
    }

    [Test]
    public void ReExecutesMigrationJobAfterRollbackAndReupdate()
    {
        using TestContext context = this.GetServer();
        IDataStore dataStore = context.GetDataStore();
        WorkerManager manager = new(Logger, dataStore, context.DatabaseProvider);
        TestMigrationJob job = new();
        manager.AddJob(job);

        GameUser user = context.CreateUser();
        GameLevel firstLevel = context.CreateLevel(user);
        
        // migrate first level
        manager.RemoveUnusedJobStates();
        manager.RunWorkCycle();
        context.Database.Refresh();

        object? stateObject = context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        MigrationJobState jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Complete, Is.True);
        Assert.That(jobState.Processed, Is.EqualTo(1));

        GameLevel? firstLevelMigrated = context.Database.GetLevelById(firstLevel.LevelId);
        Assert.That(firstLevelMigrated, Is.Not.Null);
        Assert.That(firstLevelMigrated!.Title, Does.EndWith(" test"));
        context.Database.Refresh();

        // While we do automatically apply whatever new change we've migrated existing entities to for real migrations (photo subjects, player limits etc.),
        // we don't auto-append " test" to new levels here, so we can safely create this level now, which would need migration aswell because of this.
        GameLevel secondLevel = context.CreateLevel(user);
        // Should skip job because it's still "complete", so the new level won't be migrated.
        manager.RunWorkCycle();
        context.Database.Refresh();

        GameLevel? secondLevelFromDb = context.Database.GetLevelById(secondLevel.LevelId);
        Assert.That(secondLevelFromDb, Is.Not.Null);
        Assert.That(secondLevelFromDb!.Title, Does.Not.EndWith(" test"));

        stateObject = context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Complete, Is.True);
        Assert.That(jobState.Processed, Is.EqualTo(1));

        // Simulate a roll-back, meaning the job wouldn't be in the WorkerManager anymore, so no migrations will happen, and the job state would be
        // auto-removed by WorkerManager.Start() in real cases.
        manager = new(Logger, dataStore, context.DatabaseProvider);
        manager.RemoveUnusedJobStates();
        manager.RunWorkCycle();
        context.Database.Refresh();

        // Second level was not migrated, and the job state was removed
        stateObject = context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Null);

        GameLevel? secondLevelMigrated = context.Database.GetLevelById(secondLevel.LevelId);
        Assert.That(secondLevelMigrated, Is.Not.Null);
        Assert.That(secondLevelMigrated!.Title, Does.Not.EndWith(" test"));

        // Now simulate a re-update, where the job is in the WorkerManager again
        manager = new(Logger, dataStore, context.DatabaseProvider);
        job = new();
        manager.AddJob(job);
        manager.RemoveUnusedJobStates();
        manager.RunWorkCycle();
        context.Database.Refresh();

        stateObject = context.Database.GetJobState(typeof(TestMigrationJob).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Complete, Is.True);
        Assert.That(jobState.Processed, Is.EqualTo(2));

        secondLevelMigrated = context.Database.GetLevelById(secondLevel.LevelId);
        Assert.That(secondLevelMigrated, Is.Not.Null);
        Assert.That(secondLevelMigrated!.Title, Does.EndWith(" test"));
    }
}