using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ResourceEndpoints : EndpointGroup
{
    //NOTE: type does nothing here, but it's sent by LBP so we have to accept it
    [GameEndpoint("upload/{hash}/{type}", Method.Post)]
    [GameEndpoint("upload/{hash}", Method.Post)]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public Response UploadAsset(RequestContext context, string hash, string type, byte[] body, IDataStore dataStore, GameDatabaseContext database, GameUser user, AssetImporter importer)
    {
        if (dataStore.ExistsInStore(hash))
            return Conflict;

        GameAsset? gameAsset = importer.ReadAndVerifyAsset(hash, body);
        if (gameAsset == null)
            return BadRequest;
        
        if (!dataStore.WriteToStore(hash, body))
            return InternalServerError;

        gameAsset.OriginalUploader = user;
        database.AddAssetToDatabase(gameAsset);

        return OK;
    }

    [GameEndpoint("r/{hash}")]
    public Response GetResource(RequestContext context, string hash, IDataStore dataStore)
    {
        if (!dataStore.ExistsInStore(hash))
            return NotFound;

        if (!dataStore.TryGetDataFromStore(hash, out byte[]? data))
            return InternalServerError;

        Debug.Assert(data != null);
        return new Response(data, ContentType.BinaryData);
    }

    [GameEndpoint("showNotUploaded", Method.Post, ContentType.Xml)]
    public SerializedResourceList ShowNotUploaded(RequestContext context, SerializedResourceList body, IDataStore dataStore) 
        => new(body.Items.Where(r => !dataStore.ExistsInStore(r)));
}