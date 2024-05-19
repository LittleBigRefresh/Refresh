using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class PhotoEndpoints : EndpointGroup
{
    [GameEndpoint("uploadPhoto", HttpMethods.Post, ContentType.Xml)]
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

        if (body.PhotoSubjects.Count > 4)
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, $"Too many subjects in photo, rejecting photo upload. Uploader: {user.UserId}");
            return BadRequest;
        }

        database.UploadPhoto(body, user);

        return OK;
    }

    [GameEndpoint("deletePhoto/{id}", HttpMethods.Post)]
    public Response DeletePhoto(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GamePhoto? photo = database.GetPhotoById(id);
        if (photo == null) return NotFound;

        if (photo.Publisher.UserId != user.UserId)
            return Unauthorized;
        
        database.RemovePhoto(photo);
        return OK;
    }
    
    private static Response GetPhotos(RequestContext context, GameDatabaseContext database, DataContext dataContext, Func<GameUser, int, int, DatabaseList<GamePhoto>> photoGetter)
    {
        string? username = context.QueryString.Get("user");
        if (username == null) return BadRequest;

        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return NotFound;
        
        (int skip, int count) = context.GetPageData();

        // count not used ingame
        IEnumerable<SerializedPhoto> photos = photoGetter.Invoke(user, count, skip).Items
            .Select(photo => SerializedPhoto.FromGamePhoto(photo, dataContext));

        return new Response(new SerializedPhotoList(photos), ContentType.Xml);
    }

    [GameEndpoint("photos/with", ContentType.Xml)]
    [Authentication(false)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response PhotosWithUser(RequestContext context, GameDatabaseContext database, DataContext dataContext) 
        => GetPhotos(context, database, dataContext, database.GetPhotosWithUser);
    
    [GameEndpoint("photos/by", ContentType.Xml)]
    [Authentication(false)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response PhotosByUser(RequestContext context, GameDatabaseContext database, DataContext dataContext) 
        => GetPhotos(context, database, dataContext, database.GetPhotosByUser);
}