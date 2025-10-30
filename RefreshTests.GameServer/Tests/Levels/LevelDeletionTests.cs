using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Levels;

public class LevelDeletionTests : GameServerTest
{
    private const string TEST_ASSET_HASH = "0ec63b140374ba704a58fa0c743cb357683313dd";
    
    [Test]
    public void DeletesLevel()
    {
        using TestContext context = this.GetServer();
        GameUser author = context.CreateUser();
        GameLevel? level = context.CreateLevel(author);
        
        context.Database.Refresh();
        level = context.Database.GetLevelById(level.LevelId);
        Assert.That(level, Is.Not.Null);
        
        context.Database.DeleteLevel(level!);
        
        context.Database.Refresh();
        Assert.That(context.Database.GetLevelById(level!.LevelId), Is.Null);
    }
    
    [Test]
    public void DeletesLevelWithPhotos()
    {
        using TestContext context = this.GetServer();
        GameUser? author = context.CreateUser();
        GameLevel? level = context.CreateLevel(author);
        
        context.Database.Refresh();
        level = context.Database.GetLevelById(level.LevelId);
        author = context.Database.GetUserByUsername(author.Username);
        Assert.That(level, Is.Not.Null);
        Assert.That(author, Is.Not.Null);
        
        context.Database.AddAssetToDatabase(new GameAsset
        {
            AssetHash = "0ec63b140374ba704a58fa0c743cb357683313dd",
            AssetFormat = GameAssetFormat.Binary,
            AssetType = GameAssetType.Png,
            UploadDate = DateTimeOffset.UtcNow,
        });

        SerializedPhoto photo = new()
        {
            Level = new SerializedPhotoLevel
            {
                LevelId = level!.LevelId,
                Title = level.Title,
                Type = level.SlotType.ToGameType(),
            },
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = TEST_ASSET_HASH,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = author!.Username,
            PhotoSubjects =
            [
                new SerializedPhotoSubject
                {
                    Username = author.Username,
                    DisplayName = author.Username,
                    BoundsList = "1,1,1,1",
                },

            ],
        };

        context.Database.UploadPhoto(photo, author);
        context.Database.Refresh();
        
        level = context.Database.GetLevelById(level.LevelId);
        context.Database.DeleteLevel(level!);
        
        context.Database.Refresh();
        Assert.That(context.Database.GetLevelById(level!.LevelId), Is.Null);
    }
}