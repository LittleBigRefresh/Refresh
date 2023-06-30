using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class PhotoEndpoints : EndpointGroup
{
    [GameEndpoint("uploadPhoto", Method.Post, ContentType.Xml)]
    public Response UploadPhoto(RequestContext context, SerializedPhoto body, GameDatabaseContext database, GameUser user, IDataStore dataStore)
    {
        if (!dataStore.ExistsInStore(body.SmallHash) ||
            !dataStore.ExistsInStore(body.MediumHash) ||
            !dataStore.ExistsInStore(body.LargeHash) ||
            !dataStore.ExistsInStore(body.PlanHash))
        {
            database.AddErrorNotification("Photo upload failed", "The required assets were not available.", user);
            return BadRequest;
        }

        database.UploadPhoto(body, user);

        return OK;
    }

    private static SerializedPhotoList? GetPhotos(RequestContext context, GameDatabaseContext database, Func<GameUser, int, int, DatabaseList<GamePhoto>> photoGetter)
    {
        string? username = context.QueryString.Get("user");
        if (username == null) return null;

        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;
        
        (int skip, int count) = context.GetPageData();

        // count not used ingame
        IEnumerable<SerializedPhoto> photos = photoGetter.Invoke(user, count, skip).Items
            .Select(SerializedPhoto.FromGamePhoto);

        return new SerializedPhotoList(photos);
    }

    [GameEndpoint("photos/with", ContentType.Xml)]
    [Authentication(false)]
    public SerializedPhotoList? PhotosWithUser(RequestContext context, GameDatabaseContext database) 
        => GetPhotos(context, database, database.GetPhotosWithUser);
    
    [GameEndpoint("photos/by", ContentType.Xml)]
    [Authentication(false)]
    public SerializedPhotoList? PhotosByUser(RequestContext context, GameDatabaseContext database) 
        => GetPhotos(context, database, database.GetPhotosByUser);
}