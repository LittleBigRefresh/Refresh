using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Workers;
using Refresh.Interfaces.Workers.Migrations;
using Refresh.Workers.State;

namespace RefreshTests.GameServer.Tests.Workers;

public class PhotoMigrationTests : GameServerTest
{
    private const string TEST_ASSET_HASH = "0ec63b140374ba704a58fa0c743cb357683313dd";

    [Test]
    public void MigratesPhotoSubjectsButNotMultipleTimes()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        context.Database.AddAssetToDatabase(new()
        {
            AssetHash = TEST_ASSET_HASH,
            AssetType = GameAssetType.Png,
            AssetFormat = GameAssetFormat.Unknown,
            OriginalUploader = user,
            UploadDate = new(new DateTime(2026, 3, 11), TimeSpan.Zero),
        });

        for (int i = 0; i < 10; i++)
        {
            context.Database.Add(new GamePhoto()
            {
                TakenAt = new(new DateTime(2026, 3, 18), TimeSpan.Zero),
                PublishedAt = new(new DateTime(2026, 3, 18), TimeSpan.Zero),
                PublisherId = user.UserId,
                SmallAssetHash = TEST_ASSET_HASH,
                MediumAssetHash = TEST_ASSET_HASH,
                LargeAssetHash = TEST_ASSET_HASH,
                PlanHash = "plan",
                LevelType = "pod",
                OriginalLevelId = 0,
                OriginalLevelName = "",

                #pragma warning disable CS0618 // obsoletion
                Subject1DisplayName = "p",
                Subject1Bounds = [3,1,4,1],
                Subject2DisplayName = "i",
                Subject2Bounds = [5,9,2,7],
                #pragma warning restore CS0618
            });
        }

        context.Database.SaveChanges();
        Assert.That(context.Database.GetTotalPhotoCount(), Is.EqualTo(10));

        MoveSubjectsOutOfGamePhotosMigration job = new();
        context.Database.UpdateOrCreateJobState(typeof(MoveSubjectsOutOfGamePhotosMigration).Name, new MigrationJobState()
        {
            Total = 10
        }, WorkerClass.Refresh);
        context.Database.Refresh();

        object? stateObject = context.Database.GetJobState(typeof(MoveSubjectsOutOfGamePhotosMigration).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);
        job.JobState = stateObject!;
        
        // Migrate - First cycle
        job.ExecuteJob(context.GetWorkContext());
        context.Database.Refresh();

        Assert.That(context.Database.GetTotalPhotoCount(), Is.EqualTo(10));
        foreach(GamePhoto photo in context.Database.GetRecentPhotos(10, 0).Items.ToArray())
        {
            Assert.That(photo.Subjects.Count, Is.EqualTo(2));
            Assert.That(context.Database.GetTotalSubjectsInPhoto(photo), Is.EqualTo(2));

            Assert.That(photo.Subjects.Count(s => s.DisplayName == "p"), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.DisplayName == "i"), Is.EqualTo(1));

            Assert.That(photo.Subjects.Count(s => s.PlayerId == 1), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.PlayerId == 2), Is.EqualTo(1));
        }

        stateObject = context.Database.GetJobState(typeof(MoveSubjectsOutOfGamePhotosMigration).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        MigrationJobState jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Processed, Is.EqualTo(10));
        Assert.That(jobState.Total, Is.EqualTo(10));
        Assert.That(jobState.Complete, Is.True);

        // Add new, unmigrated photos
        for (int i = 0; i < 5; i++)
        {
            context.Database.Add(new GamePhoto()
            {
                TakenAt = new(new DateTime(2026, 3, 27), TimeSpan.Zero),
                PublishedAt = new(new DateTime(2026, 3, 27), TimeSpan.Zero),
                PublisherId = user.UserId,
                SmallAssetHash = TEST_ASSET_HASH,
                MediumAssetHash = TEST_ASSET_HASH,
                LargeAssetHash = TEST_ASSET_HASH,
                PlanHash = "plan",
                LevelType = "pod",
                OriginalLevelId = 0,
                OriginalLevelName = "",

                #pragma warning disable CS0618 // obsoletion
                Subject1DisplayName = "d",
                Subject1Bounds = [1,2,3,4],
                Subject2DisplayName = "a",
                Subject2Bounds = [1,2,3,4],
                Subject3DisplayName = "n",
                Subject3Bounds = [1,2,3,4],
                Subject4DisplayName = "k",
                Subject4Bounds = [1,2,3,4],
                #pragma warning restore CS0618
            });
        }
        context.Database.SaveChanges();
        Assert.That(context.Database.GetTotalPhotoCount(), Is.EqualTo(15));
        context.Database.Refresh();

        // Reset state, to test both the migrated photos not having their subjects re-migrated (should be skipped entirely due to SortAndFilter()),
        // and to also test the new photos being migrated
        jobState.Total = 5;
        jobState.Processed = 0;
        job.JobState = jobState;
        context.Database.UpdateOrCreateJobState(typeof(MoveSubjectsOutOfGamePhotosMigration).Name, jobState, WorkerClass.Refresh);
        context.Database.Refresh();

        // Migrate - Second cycle
        job.ExecuteJob(context.GetWorkContext());
        context.Database.Refresh();

        Assert.That(context.Database.GetTotalPhotoCount(), Is.EqualTo(15));
        foreach(GamePhoto photo in context.Database.GetRecentPhotos(10, 5).Items.ToArray())
        {
            // The photos from the first cycle still only have 2 subjects each
            Assert.That(photo.Subjects.Count, Is.EqualTo(2));
            Assert.That(context.Database.GetTotalSubjectsInPhoto(photo), Is.EqualTo(2));

            Assert.That(photo.Subjects.Count(s => s.DisplayName == "p"), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.DisplayName == "i"), Is.EqualTo(1));

            Assert.That(photo.Subjects.Count(s => s.PlayerId == 1), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.PlayerId == 2), Is.EqualTo(1));
        }

        foreach(GamePhoto photo in context.Database.GetRecentPhotos(5, 0).Items.ToArray())
        {
            Assert.That(photo.Subjects.Count, Is.EqualTo(4));
            Assert.That(context.Database.GetTotalSubjectsInPhoto(photo), Is.EqualTo(4));

            Assert.That(photo.Subjects.Count(s => s.DisplayName == "d"), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.DisplayName == "a"), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.DisplayName == "n"), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.DisplayName == "k"), Is.EqualTo(1));

            Assert.That(photo.Subjects.Count(s => s.PlayerId == 1), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.PlayerId == 2), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.PlayerId == 3), Is.EqualTo(1));
            Assert.That(photo.Subjects.Count(s => s.PlayerId == 4), Is.EqualTo(1));
        }

        stateObject = context.Database.GetJobState(typeof(MoveSubjectsOutOfGamePhotosMigration).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);

        jobState = (MigrationJobState)stateObject!;
        Assert.That(jobState.Processed, Is.EqualTo(5));
        Assert.That(jobState.Total, Is.EqualTo(5));
        Assert.That(jobState.Complete, Is.True);
    }
}