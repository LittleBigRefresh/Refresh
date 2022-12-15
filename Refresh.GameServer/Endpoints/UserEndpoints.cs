using System.Xml.Serialization;
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


}