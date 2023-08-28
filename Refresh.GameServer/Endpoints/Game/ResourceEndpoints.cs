using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ResourceEndpoints : EndpointGroup
{
    //NOTE: type does nothing here, but it's sent by LBP so we have to accept it
    [GameEndpoint("upload/{hash}/{type}", Method.Post)]
    [GameEndpoint("upload/{hash}", Method.Post)]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public Response UploadAsset(RequestContext context, string hash, string type, byte[] body, IDataStore dataStore,
        GameDatabaseContext database, GameUser user, AssetImporter importer, GameServerConfig config, IDateTimeProvider timeProvider)
    {
        if (dataStore.ExistsInStore(hash))
            return Conflict;

        GameAsset? gameAsset = importer.ReadAndVerifyAsset(hash, body);
        if (gameAsset == null)
            return BadRequest;

        long earliestTime = new DateTimeOffset(2007, 0, 0, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        gameAsset.UploadDate = DateTimeOffset.FromUnixTimeSeconds(Math.Clamp(gameAsset.UploadDate.ToUnixTimeSeconds(), earliestTime, timeProvider.TimestampSeconds));

        // for example, if asset safety level is Dangerous (2) and maximum is configured as Safe (0), return 401
        // if asset safety is Safe (0), and maximum is configured as Safe (0), proceed 
        if (gameAsset.SafetyLevel > config.MaximumAssetSafetyLevel)
        {
            context.Logger.LogWarning(BunkumContext.UserContent, $"{gameAsset.AssetType} {hash} is above configured safety limit " +
                                                                 $"({gameAsset.SafetyLevel} > {config.MaximumAssetSafetyLevel})");
            return Unauthorized;
        }

        if (!dataStore.WriteToStore(hash, body))
            return InternalServerError;

        gameAsset.OriginalUploader = user;
        database.AddAssetToDatabase(gameAsset);

        return OK;
    }

    [GameEndpoint("r/{hash}")]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetResource(RequestContext context, string hash, IDataStore dataStore, GameDatabaseContext database, Token token)
    {
        if (!dataStore.ExistsInStore(hash))
            return NotFound;

        if (!dataStore.TryGetDataFromStore(hash, out byte[]? data))
            return InternalServerError;

        Debug.Assert(data != null);
        return new Response(data, ContentType.BinaryData);
    }

    [GameEndpoint("showNotUploaded", Method.Post, ContentType.Xml)]
    [GameEndpoint("filterResources", Method.Post, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedResourceList GetAssetsMissingFromStore(RequestContext context, SerializedResourceList body, IDataStore dataStore) 
        => new(body.Items.Where(r => !dataStore.ExistsInStore(r)));
}