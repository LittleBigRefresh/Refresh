using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Photos;

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
}