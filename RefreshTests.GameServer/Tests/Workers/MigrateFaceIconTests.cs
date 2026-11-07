using Bunkum.Core.Storage;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Workers;
using Refresh.Interfaces.Workers.Migrations;
using Refresh.Workers.State;

namespace RefreshTests.GameServer.Tests.Workers;

public class MigrateFaceIconTests : GameServerTest
{
    private const string IconHash = "9488801db61c5313db3bb15db7d66fd26df7e789"; // hash of "TEX "
    private const string IconHashB = "ce58c583988c5a9f60c00f8145b97a04f4734bb8"; // hash of "TEX B"
    private const string IconHashC = "b84db283f8bfb5647ebb3c3f7a91db40901fe5a0"; // hash of "TEX C"
    private const string PlanHash = "7ea4df0f7f357b33acfcda687ba8fa49a558d345"; // hash of "PLNb"
    private const string MissingTexHash = "6c050a6305ac12358e60239d1852e9d3c4e37b09"; // hash of "TEX missing"
    private const string ValidIconGuid = "g18451"; // star sticker
    private const string NonIconGuid = "g1087"; // sackboy model

    private void RunMigrationJob(TestContext context)
    {
        // Prepare migration
        ResetInvalidFaceIconsMigration job = new();
        context.Database.UpdateOrCreateJobState(typeof(ResetInvalidFaceIconsMigration).Name, new MigrationJobState()
        {
            Total = 1
        }, WorkerClass.Refresh);
        context.Database.Refresh();

        object? stateObject = context.Database.GetJobState(typeof(ResetInvalidFaceIconsMigration).Name, typeof(MigrationJobState), WorkerClass.Refresh);
        Assert.That(stateObject, Is.Not.Null);
        job.JobState = stateObject!;
        
        // Migrate
        job.ExecuteJob(context.GetWorkContext());
        context.Database.Refresh();
    }

    [Test]
    [TestCase(IconHash,       true,  "",          false, "  ",          false)] // empty and whitespaces will be corrected to 0
    [TestCase("0",            true,  IconHash,    true,  "0",           true)] // blank and hash stay the same
    [TestCase(MissingTexHash, false, PlanHash,    false, IconHash,      true)] // non-images and non-existent assets get reset
    [TestCase("g",            false, NonIconGuid, false, ValidIconGuid, false)] // GUIDs get reset, ignoring whether they're properly formatted, images, or non-images
    public void ResetFaceIconsIfInvalid(string yayHash, bool isYayValid, string mehHash, bool isMehValid, string booHash, bool isBooValid)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        // Upload both the plan and the non-missing tex
        IDataStore dataStore = context.GetDataStore();
        dataStore.WriteToStore(IconHash, "TEX "u8);
        dataStore.WriteToStore(PlanHash, "PLNb"u8);

        // Manually set to the potentially invalid hashes
        context.Database.UpdateUserFaceIcons(user, yayHash, mehHash, booHash);
        context.Database.Refresh();

        // Migrate
        this.RunMigrationJob(context);

        // Assert that the invalid icons were reset
        GameUser? updatedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.YayFaceHash, Is.EqualTo(isYayValid ? yayHash : "0"));
        Assert.That(updatedUser!.MehFaceHash, Is.EqualTo(isMehValid ? mehHash : "0"));
        Assert.That(updatedUser!.BooFaceHash, Is.EqualTo(isBooValid ? booHash : "0"));
    }

    [Test]
    public void ResetFaceIconsIfDisallowed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        // Upload the icons so they actually get reset due to being disallowed, not due to missing from the store
        IDataStore dataStore = context.GetDataStore();
        dataStore.WriteToStore(IconHash, "TEX "u8);
        dataStore.WriteToStore(IconHashB, "TEX B"u8);
        dataStore.WriteToStore(IconHashC, "TEX C"u8);

        // Disallow all of them
        context.Database.DisallowAsset(IconHash, GameAssetType.Texture, "ugly costume");
        context.Database.DisallowAsset(IconHashB, GameAssetType.Texture, "ugly costume");
        context.Database.DisallowAsset(IconHashC, GameAssetType.Texture, "ugly costume");

        // Manually set to the disallowed hashes
        context.Database.UpdateUserFaceIcons(user, IconHash, IconHashB, IconHashC);
        context.Database.Refresh();

        // Migrate
        this.RunMigrationJob(context);

        // Assert that all icons were reset
        GameUser? updatedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.YayFaceHash, Is.EqualTo("0"));
        Assert.That(updatedUser!.MehFaceHash, Is.EqualTo("0"));
        Assert.That(updatedUser!.BooFaceHash, Is.EqualTo("0"));
    }

    [Test]
    public void ResetFaceIconsIfCorrupt()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        IDataStore dataStore = context.GetDataStore();

        // Hash actually corresponds to "TEX ", so the hash we give will be incorrect, which is what we test against here.
        // We want the job to try and import the assets to DB itself here, so don't add them to DB ourselves here.
        dataStore.WriteToStore(IconHash, "TEX ABC"u8); 

        // Manually set to the incorrect hash
        context.Database.UpdateUserFaceIcons(user, IconHash, IconHash, IconHash);
        context.Database.Refresh();

        // Migrate
        this.RunMigrationJob(context);

        // Assert that all icons were reset
        GameUser? updatedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.YayFaceHash, Is.EqualTo("0"));
        Assert.That(updatedUser!.MehFaceHash, Is.EqualTo("0"));
        Assert.That(updatedUser!.BooFaceHash, Is.EqualTo("0"));
    }

    [Test]
    public void DontResetFaceIconsIfDataStoreReadsFail()
    {
        using TestContext context = this.GetServer(true, new ReadFailingDataStore());
        GameUser user = context.CreateUser();
        IDataStore dataStore = context.GetDataStore();

        // The asset will exist in the store, but ReadFailingDataStore will fail to read it.
        // In this case, the validation method returns InternalServerError, which the job treats the same as if the icon was fine.
        // This is so we don't end up resetting lots of perfectly fine icons because of filesystem issues on our end or something like that.
        // We want the job to try and import the assets to DB itself here, so don't add them to DB ourselves here.
        dataStore.WriteToStore(IconHash, "TEX "u8);
        context.Database.UpdateUserFaceIcons(user, IconHash, IconHash, IconHash);
        context.Database.Refresh();

        // Migrate
        this.RunMigrationJob(context);

        // Assert that no icons were reset
        GameUser? updatedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.YayFaceHash, Is.EqualTo(IconHash));
        Assert.That(updatedUser!.MehFaceHash, Is.EqualTo(IconHash));
        Assert.That(updatedUser!.BooFaceHash, Is.EqualTo(IconHash));
    }
}