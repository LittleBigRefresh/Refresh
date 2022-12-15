using System.Xml.Serialization;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Database.Types;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class UserEndpoints : EndpointGroup
{
    [GameEndpoint("user/{name}", Method.Get, ContentType.Xml)]
    public GameUser? GetUser(RequestContext context, RealmDatabaseContext database, string name)
    {
        GameUser? user = database.GetUser(name);
        return user;
    }

    [GameEndpoint("updateUser", Method.Post, ContentType.Xml)]
    public string? UpdateUser(RequestContext context, RealmDatabaseContext database, GameUser user, Stream body)
    {
        XmlSerializer serializer = new(typeof(UpdateUserData));
        if (serializer.Deserialize(body) is not UpdateUserData updateUserData) return null;
        
        database.UpdateUserData(user, updateUserData);

        return string.Empty;
    }

    [GameEndpoint("update_my_pins", Method.Get, ContentType.Xml)]
    public string? UpdatePins(RequestContext context, RealmDatabaseContext database, GameUser user, Stream body) 
    {
        JsonSerializer serializer = new JsonSerializer();

        using StreamReader   streamReader = new StreamReader(body);
        using JsonTextReader jsonReader   = new JsonTextReader(streamReader);
        
        UpdateUserPins? updateUserPins = serializer.Deserialize<UpdateUserPins>(jsonReader);

        //If the type is not correct, return null
        if (updateUserPins is null)
            return null;
        
        //TODO: store the updated pins in the database, and return the new pins?
        //NOTE: the returned value in the packet capture has a few higher values than the ones sent in the request,
        //      so im not sure what we are supposed to return here, so im just passing it through with `profile_pins` nulled out

        //Dont serialize profile pins, the packet capture doesnt have them in the return
        updateUserPins.ProfilePins = null!;

        //Just return the same pins back to the client
        return JsonConvert.SerializeObject(updateUserPins, new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
        });
    }
}