using System.Reflection;
using Refresh.Common.Helpers;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Interfaces.Game.Types.Lists;
using Refresh.Database.Helpers;
using System.Security.Cryptography;
using Refresh.Core.Configuration;
using Refresh.Database.Models;
using System.Text;
using System.Net;

namespace RefreshTests.GameServer.Tests.Photos;

public class PhotoEndpointsTests : GameServerTest
{
    private const string TEST_ASSET_HASH = "0ec63b140374ba704a58fa0c743cb357683313dd";
    private static readonly byte[] TestAsset = ResourceHelper.ReadResource("RefreshTests.GameServer.Resources.1x1.png", Assembly.GetExecutingAssembly());
    
    [Test]
    public void UploadAndDeletePhoto()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        SerializedPhoto photo = new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = user.Username,
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = TEST_ASSET_HASH,
            Level = new SerializedPhotoLevel
            {
                LevelId = level.LevelId,
                Title = level.Title,
                Type = "user",
            },
            PhotoSubjects = new List<SerializedPhotoSubject>
            {
                new()
                {
                    Username = user.Username,
                    DisplayName = user.Username,
                    BoundsList = "1,1,1,1",
                },
            }
        };
        
        //Upload a new photo
        message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Check for it by looking at all photos by the user
        message = client.GetAsync($"/lbp/photos/by?user={user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedPhotoList response = message.Content.ReadAsXML<SerializedPhotoList>();
        Assert.That(response.Items, Has.Count.EqualTo(1));
        Assert.That(response.Items[0].LargeHash, Is.EqualTo(TEST_ASSET_HASH));
        
        //Check for it by looking at all photos with the user
        message = client.GetAsync($"/lbp/photos/with?user={user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        response = message.Content.ReadAsXML<SerializedPhotoList>();
        Assert.That(response.Items, Has.Count.EqualTo(1));
        Assert.That(response.Items[0].LargeHash, Is.EqualTo(TEST_ASSET_HASH));
        
        //Delete the photo
        message = client.PostAsync($"/lbp/deletePhoto/{response.Items[0].PhotoId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK)); 
        
        //Make sure there are no more photos
        message = client.GetAsync($"/lbp/photos/by?user={user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        response = message.Content.ReadAsXML<SerializedPhotoList>();
        Assert.That(response.Items, Has.Count.EqualTo(0));
    }

    [Test]
    public void UploadPhotoAndValidateAttributes()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.Database.GetStoryLevelById(420);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        DateTimeOffset takenAt = context.Time.Now;
        context.Time.TimestampMilliseconds += 2000; // Increase to differentiate between creation and publish date
        
        SerializedPhoto photo = new()
        {
            Timestamp = takenAt.ToUnixTimeSeconds(),
            AuthorName = user.Username,
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = TEST_ASSET_HASH,
            Level = new SerializedPhotoLevel
            {
                LevelId = level.StoryId,
                Title = "real title",
                Type = "developer",
            },
            PhotoSubjects = new List<SerializedPhotoSubject>
            {
                new()
                {
                    Username = user.Username,
                    DisplayName = user.Username,
                    BoundsList = "1,1,1,1",
                },
                new()
                {
                    Username = "SecretAlt",
                    DisplayName = "SecretAlt",
                    BoundsList = "2,4,5,6",
                },
            }
        };
        message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Get the photo
        GamePhoto? gamePhoto = context.Database.GetPhotosByUser(user, 100, 0).Items.FirstOrDefault();
        Assert.That(gamePhoto, Is.Not.Null);

        Assert.That(gamePhoto!.PublisherId, Is.EqualTo(user.UserId));
        Assert.That(gamePhoto!.Publisher.UserId, Is.EqualTo(user.UserId));
        Assert.That(gamePhoto!.PublisherId, Is.EqualTo(user.UserId));

        Assert.That(gamePhoto!.TakenAt, Is.EqualTo(takenAt));
        Assert.That(gamePhoto!.PublishedAt, Is.EqualTo(context.Time.Now));
        Assert.That(gamePhoto!.PublishedAt, Is.Not.EqualTo(gamePhoto.TakenAt)); // Should be apart by 2 seconds (see above)

        Assert.That(gamePhoto!.Level!, Is.Not.Null);
        Assert.That(gamePhoto!.Level!.LevelId, Is.EqualTo(level.LevelId));
        Assert.That(gamePhoto!.LevelId, Is.EqualTo(level.LevelId));
        Assert.That(gamePhoto!.OriginalLevelId, Is.EqualTo(level.StoryId));
        Assert.That(gamePhoto!.LevelType, Is.EqualTo("developer"));
        Assert.That(gamePhoto!.OriginalLevelName, Is.EqualTo("real title"));

        Assert.That(gamePhoto!.SmallAssetHash, Is.EqualTo(TEST_ASSET_HASH));
        Assert.That(gamePhoto!.MediumAssetHash, Is.EqualTo(TEST_ASSET_HASH));
        Assert.That(gamePhoto!.LargeAssetHash, Is.EqualTo(TEST_ASSET_HASH));
        Assert.That(gamePhoto!.PlanHash, Is.EqualTo(TEST_ASSET_HASH));

        List<GamePhotoSubject> subjects = context.Database.GetSubjectsInPhoto(gamePhoto).ToList();

        Assert.That(subjects.Count, Is.EqualTo(2));
        Assert.That(subjects[0].Bounds, Is.EqualTo(PhotoHelper.ParseBoundsList("1,1,1,1")));
        Assert.That(subjects[0].DisplayName, Is.EqualTo(user.Username));
        Assert.That(subjects[0].User, Is.Not.Null);
        Assert.That(subjects[0].User!.UserId, Is.EqualTo(user.UserId));

        Assert.That(subjects[1].Bounds, Is.EqualTo(PhotoHelper.ParseBoundsList("2,4,5,6")));
        Assert.That(subjects[1].DisplayName, Is.EqualTo("SecretAlt"));
        Assert.That(subjects[1].User, Is.Null);
    }
    
    [Test]
    public void CantUploadPhotoWithMissingAssets()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        SerializedPhoto photo = new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = user.Username,
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = TEST_ASSET_HASH,
            Level = new SerializedPhotoLevel
            {
                LevelId = level.LevelId,
                Title = level.Title,
                Type = "user",
            },
            PhotoSubjects = new List<SerializedPhotoSubject>
            {
                new()
                {
                    Username = user.Username,
                    DisplayName = user.Username,
                    BoundsList = "1,1,1,1",
                },
            }
        };
        
        HttpResponseMessage message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantUploadPhotoWithTooManySubjects()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        SerializedPhoto photo = new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = user.Username,
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = TEST_ASSET_HASH,
            Level = new SerializedPhotoLevel
            {
                LevelId = level.LevelId,
                Title = level.Title,
                Type = "user",
            },
            PhotoSubjects = new List<SerializedPhotoSubject>
            {
                new()
                {
                    Username = user.Username,
                    DisplayName = user.Username,
                    BoundsList = "1,1,1,1",
                },
                new()
                {
                    Username = user.Username,
                    DisplayName = user.Username,
                    BoundsList = "1,1,1,1",
                },
                new()
                {
                    Username = user.Username,
                    DisplayName = user.Username,
                    BoundsList = "1,1,1,1",
                },
                new()
                {
                    Username = user.Username,
                    DisplayName = user.Username,
                    BoundsList = "1,1,1,1",
                },
                new()
                {
                    Username = user.Username,
                    DisplayName = user.Username,
                    BoundsList = "1,1,1,1",
                },
            }
        };
        
        message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantGetPhotosWhenNoUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Check for it by looking at all photos by the user
        HttpResponseMessage message = client.GetAsync($"/lbp/photos/by").Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));

        //Check for it by looking at all photos with the user
        message = client.GetAsync($"/lbp/photos/with").Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantGetPhotosWhenInvalidUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.GetAsync($"/lbp/photos/by?user=IM_NOT_REAL").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));

        message = client.GetAsync($"/lbp/photos/with?user=IM_NOT_REAL").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantDeleteInvalidPhoto()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/deletePhoto/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantDeleteOthersPhoto()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user1);

        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, user1);
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, user2);

        //Upload our """photo"""
        HttpResponseMessage message = client1.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        SerializedPhoto photo = new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = user1.Username,
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = TEST_ASSET_HASH,
            Level = new SerializedPhotoLevel
            {
                LevelId = level.LevelId,
                Title = level.Title,
                Type = "user",
            },
            PhotoSubjects = new List<SerializedPhotoSubject>
            {
                new()
                {
                    Username = user1.Username,
                    DisplayName = user1.Username,
                    BoundsList = "1,1,1,1",
                },
            }
        };
        
        //Upload a new photo as user 1
        message = client1.PostAsync($"/lbp/uploadPhoto", new StringContent(photo.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Check for it by looking at all photos by the user
        message = client1.GetAsync($"/lbp/photos/by?user={user1.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedPhotoList response = message.Content.ReadAsXML<SerializedPhotoList>();
        Assert.That(response.Items, Has.Count.EqualTo(1));
        Assert.That(response.Items[0].LargeHash, Is.EqualTo(TEST_ASSET_HASH));
        
        //Delete the photo as user 2
        message = client2.PostAsync($"/lbp/deletePhoto/{response.Items[0].PhotoId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized)); 
        
        //Make sure the photo is still there
        message = client1.GetAsync($"/lbp/photos/by?user={user1.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        response = message.Content.ReadAsXML<SerializedPhotoList>();
        Assert.That(response.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public void CannotUploadPhotoIfDuplicateImage()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        //Upload our """plan"""
        ReadOnlySpan<byte> planData = "PLNbum"u8;
        string planHash = HexHelper.BytesToHexString(SHA1.HashData(planData));
        message = client.PostAsync("/lbp/upload/" + planHash, new ByteArrayContent(planData.ToArray())).Result;

        //Upload another """plan"""
        ReadOnlySpan<byte> anotherPlanData = "PLNble"u8;
        string anotherPlanHash = HexHelper.BytesToHexString(SHA1.HashData(anotherPlanData));
        message = client.PostAsync("/lbp/upload/" + anotherPlanHash, new ByteArrayContent(anotherPlanData.ToArray())).Result;
        
        SerializedPhoto photo1 = new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = user.Username,
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = planHash,
            Level = new SerializedPhotoLevel
            {
                LevelId = 0,
                Title = "",
                Type = "pod",
            }
        };
        
        //Upload photo once
        message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo1.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        //Upload photo again (different plan hash)
        SerializedPhoto photo2 = new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = user.Username,
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = anotherPlanHash,
            Level = new SerializedPhotoLevel
            {
                LevelId = 0,
                Title = "",
                Type = "pod",
            }
        };
        message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo2.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.EqualTo(1));
    }

    [Test]
    public void CannotUploadPhotoIfDuplicatePlan()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        //Upload another """photo"""
        ReadOnlySpan<byte> anotherTextureData = "TEX r"u8;
        string anotherTextureHash = HexHelper.BytesToHexString(SHA1.HashData(anotherTextureData));
        message = client.PostAsync("/lbp/upload/" + anotherTextureHash, new ByteArrayContent(anotherTextureData.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        //Upload our """plan"""
        ReadOnlySpan<byte> planData = "PLNb"u8;
        string planHash = HexHelper.BytesToHexString(SHA1.HashData(planData));
        message = client.PostAsync("/lbp/upload/" + planHash, new ByteArrayContent(planData.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        SerializedPhoto photo1 = new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = user.Username,
            SmallHash = TEST_ASSET_HASH,
            MediumHash = TEST_ASSET_HASH,
            LargeHash = TEST_ASSET_HASH,
            PlanHash = planHash,
            Level = new SerializedPhotoLevel
            {
                LevelId = 0,
                Title = "",
                Type = "pod",
            }
        };
        
        //Upload photo once
        message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo1.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        //Upload photo again (different image hashes)
        SerializedPhoto photo2 = new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AuthorName = user.Username,
            SmallHash = anotherTextureHash,
            MediumHash = anotherTextureHash,
            LargeHash = anotherTextureHash,
            PlanHash = planHash,
            Level = new SerializedPhotoLevel
            {
                LevelId = 0,
                Title = "",
                Type = "pod",
            }
        };
        message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo2.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.EqualTo(1));
    }

    [Test]
    [TestCase(4, 4, 1)]
    public void PhotoUploadsGetRateLimitedTemporarily(int photoQuota, int uploadAttemptsAfterExceeding, int timeSpanHours)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser("thepublisher");
        GameLevel level = context.CreateLevel(user);

        // Prepare config
        GameServerConfig config = context.Server.Value.GameServerConfig;
        EntityUploadRateLimitProperties uploadConfig = new()
        {
            Enabled = true,
            UploadQuota = photoQuota,
            TimeSpanHours = timeSpanHours,
        };
        config.NormalUserPermissions.PhotoUploadRateLimit = uploadConfig;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        int publishAttempts = 0;

        // Not blocked yet
        Assert.That(context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Photo), Is.Null);
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Photo, uploadConfig.UploadQuota), Is.Null);

        // Fill up half of quota
        publishAttempts += SpamUploadUniquePhotos(uploadConfig.UploadQuota / 2, client, publishAttempts, level);
        context.Database.Refresh();

        // There is rate-limit data in DB, but the rate-limit hasn't been triggered yet
        EntityUploadRateLimit? uploadLimit = context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Photo);
        Assert.That(uploadLimit, Is.Not.Null);
        Assert.That(uploadLimit!.UploadCount, Is.EqualTo(uploadConfig.UploadQuota / 2));
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Photo, uploadConfig.UploadQuota), Is.Null);
        
        // Try to upload more photos to reach quota
        publishAttempts += SpamUploadUniquePhotos(uploadConfig.UploadQuota / 2, client, publishAttempts, level);
        context.Database.Refresh();

        // Ensure there were no notifications sent
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.Zero);

        // There is rate-limit data in DB, and the rate-limit has been triggered
        uploadLimit = context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Photo);
        Assert.That(uploadLimit, Is.Not.Null);
        Assert.That(uploadLimit!.UploadCount, Is.EqualTo(uploadConfig.UploadQuota));
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Photo, uploadConfig.UploadQuota), Is.Not.Null);

        // photos are blocked
        publishAttempts += SpamUploadUniquePhotos(uploadAttemptsAfterExceeding, client, publishAttempts, level, Unauthorized);
        context.Database.Refresh();

        // Check amount of photos, and ensure there were notifications sent for every failed photo
        Assert.That(context.Database.GetTotalPhotosByUser(user), Is.EqualTo(uploadConfig.UploadQuota));
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.EqualTo(uploadAttemptsAfterExceeding));

        // Expire limit naturally by trying to publish again later
        context.Time.TimestampMilliseconds = 1000 * 60 * 60 * timeSpanHours + 10;
        publishAttempts += SpamUploadUniquePhotos(uploadAttemptsAfterExceeding, client, publishAttempts, level);
        context.Database.Refresh();

        // there are more levels now, and no new notifications
        Assert.That(context.Database.GetTotalPhotosByUser(user), Is.EqualTo(uploadConfig.UploadQuota * 2));
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.EqualTo(uploadAttemptsAfterExceeding));
    }

    private int SpamUploadUniquePhotos(int uploads, HttpClient client, int startIndex, GameLevel level, HttpStatusCode expectedStatus = OK)
    {
        for (int i = 0; i < uploads; i++)
        {
            // Upload level
            SerializedPhoto photo = PrepareUniquePhotoUploadRequest(client, level, startIndex + i);

            HttpResponseMessage message = client.PostAsync($"/lbp/uploadPhoto", new StringContent(photo.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(expectedStatus));
        }

        return uploads;
    }

    private SerializedPhoto PrepareUniquePhotoUploadRequest(HttpClient client, GameLevel level, int uniqueValue)
    {
        // upload """photo"""
        ReadOnlySpan<byte> imageResource = new(Encoding.ASCII.GetBytes($"TEX  {uniqueValue}"));
        string imageHash = BitConverter.ToString(SHA1.HashData(imageResource)).Replace("-", "").ToLower();
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{imageHash}", new ReadOnlyMemoryContent(imageResource.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // upload """plan"""
        ReadOnlySpan<byte> planResource = new(Encoding.ASCII.GetBytes($"PLNb {uniqueValue}"));
        string planHash = BitConverter.ToString(SHA1.HashData(planResource)).Replace("-", "").ToLower();
        message = client.PostAsync($"/lbp/upload/{planHash}", new ReadOnlyMemoryContent(planResource.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // prepare request body
        return new()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            SmallHash = imageHash,
            MediumHash = imageHash,
            LargeHash = imageHash,
            PlanHash = planHash,
            Level = new SerializedPhotoLevel
            {
                LevelId = level.LevelId,
                Title = level.Title,
                Type = "user",
            },
        };
    }
}