using System.Diagnostics;
using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Commands;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Filtering;

namespace Refresh.GameServer.Endpoints.Game;

public class UserEndpoints : EndpointGroup
{
    [GameEndpoint("user/{name}", Method.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public GameUserResponse? GetUser(RequestContext context, GameDatabaseContext database, string name, Token token) 
        => GameUserResponse.FromOldWithExtraData(database.GetUserByUsername(name), token.TokenGame);

    [GameEndpoint("users", Method.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedUserList GetMultipleUsers(RequestContext context, GameDatabaseContext database, Token token)
    {
        string[]? usernames = context.QueryString.GetValues("u");
        if (usernames == null) return new SerializedUserList();

        List<GameUserResponse> users = new(usernames.Length);

        foreach (string username in usernames)
        {
            GameUser? user = database.GetUserByUsername(username);
            if (user == null) continue;
            
            users.Add(GameUserResponse.FromOldWithExtraData(user, token.TokenGame)!);
        }

        return new SerializedUserList
        {
            Users = users,
        };
    }

    [GameEndpoint("myFriends", Method.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedFriendsList? GetFriends(RequestContext context, GameDatabaseContext database,
        GameUser user, FriendStorageService friendService, Token token)
    {
        List<GameUser>? friends = friendService.GetUsersFriends(user, database)?.ToList();
        if (friends == null) return null;
        
        return new SerializedFriendsList(GameUserResponse.FromOldListWithExtraData(friends, token.TokenGame).ToList());
    }

    [GameEndpoint("updateUser", Method.Post, ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    public string? UpdateUser(RequestContext context, GameDatabaseContext database, GameUser user, string body, IDataStore dataStore, Token token)
    {
        SerializedUpdateData? data = null;
        
        // This stupid shit is caused by LBP sending two different root elements for this endpoint
        // LBP is just fantastic man
        try
        {
            XmlSerializer profileSerializer = new(typeof(SerializedUpdateDataProfile));
            if (profileSerializer.Deserialize(new StringReader(body)) is not SerializedUpdateDataProfile profileData) return null;
            data = profileData;
            
            XmlSerializer planetSerializer = new(typeof(SerializedUpdateDataPlanets));
            if (planetSerializer.Deserialize(new StringReader(body)) is not SerializedUpdateDataPlanets planetsData) return null;
            data = planetsData;
        }
        catch
        {
            // ignored
        }
        
        if (data == null)
        {
            database.AddErrorNotification("Profile update failed", "Your profile failed to update because the data could not be read.", user);
            return null;
        }

        if (data.IconHash != null && !dataStore.ExistsInStore(data.IconHash))
        {
            database.AddErrorNotification("Profile update failed", "Your avatar failed to update because the asset was missing on the server.", user);
            return null;
        }
        
        if (data.PlanetsHash != null && !dataStore.ExistsInStore(data.PlanetsHash))
        {
            database.AddErrorNotification("Profile update failed", "Your planets failed to update because the asset was missing on the server.", user);
            return null;
        }

        const int maxDescriptionLength = 4096;
        if (data.Description is { Length: > maxDescriptionLength })
        {
            database.AddErrorNotification("Profile update failed", $"Your profile failed to update because the description was too long. The max length is {maxDescriptionLength}.", user);
            return null;
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (token.TokenGame)
        {
            case TokenGame.LittleBigPlanet2:
                data.Lbp2PlanetsHash = data.PlanetsHash;
                data.Lbp3PlanetsHash = data.PlanetsHash;
                break;
            case TokenGame.LittleBigPlanetVita:
                data.VitaPlanetsHash = data.PlanetsHash;
                break;
            case TokenGame.LittleBigPlanet3:
                data.Lbp3PlanetsHash = data.PlanetsHash;
                break;
        }
        
        database.UpdateUserData(user, data);
        return string.Empty;
    }

    [GameEndpoint("update_my_pins", Method.Post, ContentType.Json)]
    [NullStatusCode(BadRequest)]
    public string? UpdatePins(RequestContext context, GameDatabaseContext database, GameUser user, Stream body)
    {
        JsonSerializer serializer = new();

        using StreamReader streamReader = new(body);
        using JsonTextReader jsonReader = new(streamReader);

        UserPins? updateUserPins = serializer.Deserialize<UserPins>(jsonReader);

        //If the type is not correct, return null
        if (updateUserPins is null)
        {
            database.AddErrorNotification("Pin sync failed", "Your pins failed to update because the data could not be read.", user);
            return null;
        }
        
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
    [MinimumRole(GameUserRole.Restricted)]
    public UserPins GetPins(RequestContext context, GameUser user) => user.Pins;

    /// <summary>
    /// Censor ("filter") strings sent by the client. Used for chat messages, speech bubble contents, etc.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="body">The string to censor.</param>
    /// <param name="user">The user saying the string. Used for logging</param>
    /// <returns>The string shown in-game.</returns>
    [GameEndpoint("filter", Method.Post)]
    [AllowEmptyBody]
    public string Filter(RequestContext context, CommandService commandService, string body, GameUser user, GameDatabaseContext database)
    {
        // TODO: Add actual filtering/censoring
        
        if (commandService.IsPublishing(user.UserId))
        {
            context.Logger.LogInfo(BunkumContext.UserLevels, $"Publish filter: '{body}'");
        }
        else
        {
            context.Logger.LogInfo(BunkumContext.Filter, $"<{user}>: {body}");

            try
            {
                CommandInvocation command = commandService.ParseCommand(body);
                
                context.Logger.LogInfo(BunkumContext.Commands, $"User used command '{command.Name}' with args '{command.Arguments}'");

                commandService.HandleCommand(command, database, user);
            }
            catch
            {
                //do nothing
            } 
        }
        
        return body;
    }

    [GameEndpoint("filter/batch", Method.Post, ContentType.Xml)]
    public SerializedTextList BatchFilter(RequestContext context, SerializedTextList body)
    {
        return body;
    }
}