using System.Reflection;
using Refresh.Common.Helpers;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Interfaces.Game.Types.Lists;
using Refresh.Database.Helpers;

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

        Assert.That(gamePhoto!.Subjects.Count, Is.EqualTo(2));
        Assert.That(gamePhoto!.Subjects[0].Bounds, Is.EqualTo(PhotoHelper.ParseBoundsList("1,1,1,1")));
        Assert.That(gamePhoto!.Subjects[0].DisplayName, Is.EqualTo(user.Username));
        Assert.That(gamePhoto!.Subjects[0].User, Is.Not.Null);
        Assert.That(gamePhoto!.Subjects[0].User!.UserId, Is.EqualTo(user.UserId));

        Assert.That(gamePhoto!.Subjects[1].Bounds, Is.EqualTo(PhotoHelper.ParseBoundsList("2,4,5,6")));
        Assert.That(gamePhoto!.Subjects[1].DisplayName, Is.EqualTo("SecretAlt"));
        Assert.That(gamePhoto!.Subjects[1].User, Is.Null);
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
}