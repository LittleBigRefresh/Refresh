using System.Diagnostics;
using System.Net;
using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class UserEndpoints : EndpointGroup
{
    [GameEndpoint("user/{name}", Method.Get, ContentType.Xml)]
    public GameUser? GetUser(RequestContext context, RealmDatabaseContext database, string name)
    {
        GameUser? user = database.GetUserByUsername(name);
        return user;
    }

    [GameEndpoint("users", Method.Get, ContentType.Xml)]
    public GameUserList GetMultipleUsers(RequestContext context, RealmDatabaseContext database)
    {
        string[]? usernames = context.QueryString.GetValues("u");
        if (usernames == null) return new GameUserList();

        List<GameUser> users = new(usernames.Length);

        foreach (string username in usernames)
        {
            GameUser? user = database.GetUserByUsername(username);
            if (user == null) continue;

            user.PrepareForSerialization();
            users.Add(user);
        }

        return new GameUserList
        {
            Users = users,
        };
    }

    [GameEndpoint("updateUser", Method.Post, ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public string? UpdateUser(RequestContext context, RealmDatabaseContext database, GameUser user, string body)
    {
        UpdateUserData? data = null;
        
        // This stupid shit is caused by LBP sending two different root elements for this endpoint
        // LBP is just fantastic man
        try
        {
            XmlSerializer serializer = new(typeof(UpdateUserDataProfile));
            if (serializer.Deserialize(new StringReader(body)) is not UpdateUserDataProfile profileData) return null;
            data = profileData;
        }
        catch
        {
            // ignored
        }
        
        try
        {
            XmlSerializer serializer = new(typeof(UpdateUserDataPlanets));
            if (serializer.Deserialize(new StringReader(body)) is not UpdateUserDataPlanets planetsData) return null;
            data = planetsData;
        }
        catch
        {
            // ignored
        }

        if (data == null) return null;
        
        database.UpdateUserData(user, data);
        return string.Empty;
    }

    [GameEndpoint("update_my_pins", Method.Post, ContentType.Json)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public string? UpdatePins(RequestContext context, RealmDatabaseContext database, GameUser user, Stream body)
    {
        JsonSerializer serializer = new();

        using StreamReader streamReader = new(body);
        using JsonTextReader jsonReader = new(streamReader);

        UserPins? updateUserPins = serializer.Deserialize<UserPins>(jsonReader);

        //If the type is not correct, return null
        if (updateUserPins is null)
            return null;

        //NOTE: the returned value in the packet capture has a few higher values than the ones sent in the request,
        //      so im not sure what we are supposed to return here, so im just passing it through with `profile_pins` nulled out
        database.UpdateUserPins(user, updateUserPins);

        //Dont serialize profile pins, the packet capture doesnt have them in the return
        updateUserPins.ProfilePins?.Clear();

        //Just return the same pins back to the client
        return JsonConvert.SerializeObject(updateUserPins, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        });
    }

    [GameEndpoint("get_my_pins", Method.Get, ContentType.Json)]
    public UserPins GetPins(RequestContext context, GameUser user) => user.Pins;

    /// <summary>
    /// Censor ("filter") strings sent by the client. Used for chat messages, speech bubble contents, etc.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="body">The string to censor.</param>
    /// <param name="user">The user saying the string. Used for logging</param>
    /// <returns>The string shown in-game.</returns>
    [GameEndpoint("filter", Method.Post)]
    public string Filter(RequestContext context, string body, GameUser user)
    {
        Debug.Assert(user != null);
        Debug.Assert(body != null);
        
        string msg = $"<{user}>: {body}"; // For some reason, the logger breaks if we put this directly into the call
        context.Logger.LogInfo(BunkumContext.Filter, msg);
        return body;
    }
}