using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Lists;
using Refresh.Interfaces.Game.Types.Pins;
using Refresh.Interfaces.Game.Types.UserData;

namespace Refresh.Interfaces.Game.Endpoints;

public class UserEndpoints : EndpointGroup
{
    [GameEndpoint("user/{name}", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public GameUserResponse? GetUser(RequestContext context, GameDatabaseContext database, string name, Token token,
        IDataStore dataStore, DataContext dataContext) 
        => GameUserResponse.FromOld(database.GetUserByUsername(name), dataContext);

    [GameEndpoint("users", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedUserList GetMultipleUsers(RequestContext context, GameDatabaseContext database, Token token,
        IDataStore dataStore, DataContext dataContext)
    {
        string[]? usernames = context.QueryString.GetValues("u");
        if (usernames == null) return new SerializedUserList();

        List<GameUserResponse> users = new(usernames.Length);

        foreach (string username in usernames)
        {
            GameUser? user = database.GetUserByUsername(username);
            if (user == null) continue;
            
            users.Add(GameUserResponse.FromOld(user, dataContext)!);
        }

        return new SerializedUserList
        {
            Users = users,
        };
    }

    [GameEndpoint("myFriends", HttpMethods.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedFriendsList GetFriends(RequestContext context, GameDatabaseContext database,
        GameUser user, DataContext dataContext)
    {
        List<GameUser> friends = database.GetUsersMutuals(user).ToList();
        return new SerializedFriendsList(GameUserResponse.FromOldList(friends, dataContext).ToList());
    }

    [GameEndpoint("updateUser", HttpMethods.Post, ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    public string? UpdateUser(RequestContext context, DataContext dataContext, GameUser user, string body, GuidCheckerService guidChecker)
    {
        SerializedUpdateData? data = null;
        
        // This stupid shit is caused by LBP sending two different root elements for this endpoint
        // LBP is just fantastic man
        try
        {
            XmlSerializer profileSerializer = new(typeof(SerializedUpdateDataProfile));
            if (profileSerializer.Deserialize(new StringReader(body)) is not SerializedUpdateDataProfile profileData)
                return null;
            
            data ??= profileData;
        }
        catch
        {
            // ignored
        }

        try
        {
            XmlSerializer planetSerializer = new(typeof(SerializedUpdateDataPlanets));
            if (planetSerializer.Deserialize(new StringReader(body)) is not SerializedUpdateDataPlanets planetsData)
                return null;
            
            data ??= planetsData;
        }
        catch
        {
            // ignored
        }
        
        if (data == null)
        {
            dataContext.Database.AddErrorNotification("Profile update failed", "Your profile failed to update because the data could not be read.", user);
            return null;
        }

        if (data.IconHash != null)
        {
            //If the icon is a GUID
            if (data.IconHash.StartsWith('g'))
            {
                //Parse out the GUID
                long guid = long.Parse(data.IconHash.AsSpan()[1..]);
                
                //If its not a valid GUID, block the request
                if (data.IconHash.StartsWith('g') && !guidChecker.IsTextureGuid(dataContext.Game, guid))
                {
                    dataContext.Database.AddErrorNotification("Profile update failed", "Your avatar failed to update because the asset was an invalid GUID.", user);
                    return null; 
                }
            }
            else
            {
                //If the asset does not exist on the server, block the request
                if (!dataContext.DataStore.ExistsInStore(data.IconHash))
                {
                    dataContext.Database.AddErrorNotification("Profile update failed", "Your avatar failed to update because the asset was missing on the server.", user);
                    return null;
                } 
            }
        }
        
        if (data.LevelLocations != null && data.LevelLocations.Count > 0)
        {
            dataContext.Database.UpdateLevelLocations(data.LevelLocations, user);
        }
        
        if (!string.IsNullOrEmpty(data.PlanetsHash) && data.PlanetsHash != "0" /* Empty planets */ && !dataContext.DataStore.ExistsInStore(data.PlanetsHash))
        {
            dataContext.Database.AddErrorNotification("Profile update failed", "Your planets failed to update because the asset was missing on the server.", user);
            return null;
        }

        if (data.Description is { Length: > UgcLimits.DescriptionLimit })
        {
            dataContext.Database.AddErrorNotification("Profile update failed", $"Your profile failed to update because the description was too long. The max length is {UgcLimits.DescriptionLimit} characters.", user);
            return null;
        }
        
        dataContext.Database.UpdateUserData(user, data, dataContext.Game);
        return string.Empty;
    }

    [GameEndpoint("update_my_pins", HttpMethods.Post, ContentType.Json)]
    [RequireEmailVerified]
    [NullStatusCode(BadRequest)]
    public SerializedPins? UpdatePins(RequestContext context, DataContext dataContext, GameUser user, SerializedPins body)
    {
        // Try to convert pin progress
        Dictionary<long, int> pinProgresses = [];
        try
        {
            pinProgresses = SerializedPins.ToMergedDictionary
            ([
                SerializedPins.ToDictionary(body.ProgressPins),
                SerializedPins.ToDictionary(body.AwardPins),
            ]);
        }
        catch (Exception ex)
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, $"Failed to convert pins from list to dictionary: {ex}");
            dataContext.Database.AddErrorNotification
            (
                "Pin progress update failed",
                $"Your pin progress failed to get saved on the server because the data could not be read.",
                user
            );
            return null;
        }

        if (pinProgresses.Count > 0)
        {
            dataContext.Database.UpdateUserPinProgress(pinProgresses, user, dataContext.Game);
        }

        // Users can only have 3 pins set on their profile
        if (body.ProfilePins.Count > 3)
            return null;

        if (body.ProfilePins.Count > 0)
        {
            dataContext.Database.UpdateUserProfilePins(body.ProfilePins, user, dataContext.Game);
        }

        // Return newly updated pins (LBP2 and 3 update their pin progresses if there are higher progress values
        // in the response, but seemingly ignore the profile pins in the response)
        return this.GetPins(context, dataContext, user);
    }

    [GameEndpoint("get_my_pins", HttpMethods.Get, ContentType.Json)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedPins GetPins(RequestContext context, DataContext dataContext, GameUser user)
        => SerializedPins.FromOld
        (
            dataContext.Database.GetPinProgressesByUser(user, dataContext.Game, 0, 999).Items
        );
}