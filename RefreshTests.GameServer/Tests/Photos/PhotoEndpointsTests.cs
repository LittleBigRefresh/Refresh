using System.Reflection;
using Refresh.Common.Helpers;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;

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
            Level = new SerializedLevelIdTypeName
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
            Level = new SerializedLevelIdTypeName
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
            Level = new SerializedLevelIdTypeName
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
            Level = new SerializedLevelIdTypeName
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