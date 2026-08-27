using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Helpers;
using Refresh.Core.Importing;
using Refresh.Core.RateLimits.Users;
using Refresh.Core.Services;
using Refresh.Core.Types.Assets.Validation;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Assets;
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
    [RateLimitSettings(SingleUserEndpointLimits.TimeoutDuration, SingleUserEndpointLimits.GameRequestAmount, 
                            SingleUserEndpointLimits.BlockDuration, SingleUserEndpointLimits.GameRequestBucket)]
    public GameUserResponse? GetUser(RequestContext context, GameDatabaseContext database, string name, Token token,
        IDataStore dataStore, DataContext dataContext) 
        => GameUserResponse.FromOld(database.GetUserByUsername(name), dataContext);

    [GameEndpoint("users", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(UserListEndpointLimits.TimeoutDuration, UserListEndpointLimits.RequestAmount, 
                            UserListEndpointLimits.BlockDuration, UserListEndpointLimits.RequestBucket)]
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
    [RateLimitSettings(UserListEndpointLimits.TimeoutDuration, UserListEndpointLimits.RequestAmount, 
                            UserListEndpointLimits.BlockDuration, UserListEndpointLimits.RequestBucket)]
    public SerializedFriendsList GetFriends(RequestContext context, GameDatabaseContext database,
        GameUser user, DataContext dataContext)
    {
        List<GameUser> friends = database.GetUsersMutuals(user).ToList();
        return new SerializedFriendsList(GameUserResponse.FromOldList(friends, dataContext).ToList());
    }

    [GameEndpoint("updateUser", HttpMethods.Post, ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    [RateLimitSettings(UserModificationEndpointLimits.TimeoutDuration, UserModificationEndpointLimits.GameRequestAmount, 
                            UserModificationEndpointLimits.BlockDuration, UserModificationEndpointLimits.GameRequestBucket)]
    public Response UpdateUser(RequestContext context, DataContext dataContext, GameUser user, string body, GuidCheckerService guidChecker, 
        AssetImporter importer, AipiService? aipi)
    {
        SerializedUpdateData? data = null;
        
        // This stupid shit is caused by LBP sending two different root elements for this endpoint
        // LBP is just fantastic man
        try
        {
            XmlSerializer profileSerializer = new(typeof(SerializedUpdateDataProfile));
            if (profileSerializer.Deserialize(new StringReader(body)) is not SerializedUpdateDataProfile profileData)
                return BadRequest;
            
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
                return BadRequest;
            
            data ??= planetsData;
        }
        catch
        {
            // ignored
        }
        
        if (data == null)
        {
            dataContext.Database.AddErrorNotification("Profile update failed", "Your profile failed to update because the data could not be read.", user);
            return BadRequest;
        }

        if (data.IconHash != null)
        {
            ValidatedAssetResult iconResult = ResourceValidationHelper.ValidateReference(new(data.IconHash, dataContext, importer, aipi)
            {
                MustBeTexture = true,
                AssetContextTypeStr = "avatar",
            }, context.Logger);
            data.IconHash = iconResult.NewAssetRef;
            
            if (iconResult.Status != OK)
            {
                if (iconResult.ErrorMessage != null) dataContext.Database.AddErrorNotification("Profile update failed", iconResult.ErrorMessage, user);
                return iconResult.Status;
            }
        }
        
        AssetValidationParameters faceParams = new(null!, dataContext, importer, aipi)
        {
            MayBeBlank = false,
            MayBeGuid = false,
            MustBeTexture = true,
            AssetContextTypeStr = "image",
        };

        if (data.YayFaceHash != null)
        {
            faceParams.AssetRef = data.YayFaceHash;
            ValidatedAssetResult yayResult = ResourceValidationHelper.ValidateReference(faceParams, context.Logger);
            data.YayFaceHash = yayResult.NewAssetRef;

            if (yayResult.Status != OK) return yayResult.Status; // no need to notify, these are always updated in the background
        }

        if (data.MehFaceHash != null)
        {
            faceParams.AssetRef = data.MehFaceHash;
            ValidatedAssetResult mehResult = ResourceValidationHelper.ValidateReference(faceParams, context.Logger);
            data.MehFaceHash = mehResult.NewAssetRef;

            if (mehResult.Status != OK) return mehResult.Status;
        }

        if (data.BooFaceHash != null)
        {
            faceParams.AssetRef = data.BooFaceHash;
            ValidatedAssetResult booResult = ResourceValidationHelper.ValidateReference(faceParams, context.Logger);
            data.BooFaceHash = booResult.NewAssetRef;

            if (booResult.Status != OK) return booResult.Status;
        }

        if (data.PlanetsHash != null)
        {
            // Some LBP2 alpha builds like to insert newlines here
            data.PlanetsHash = data.PlanetsHash.Replace("\n", "");

            ValidatedAssetResult planetResult = ResourceValidationHelper.ValidateReference(new(data.PlanetsHash, dataContext, importer)
            { 
                // blank = reset planets, but GUIDs should never happen
                MayBeGuid = false,
                AssetContextTypeStr = "planet asset",
            }, context.Logger);
            data.PlanetsHash = planetResult.NewAssetRef;
            
            if (planetResult.Status != OK)
            {
                if (planetResult.ErrorMessage != null) dataContext.Database.AddErrorNotification("Planet update failed", planetResult.ErrorMessage, user);
                return planetResult.Status;
            }
            // TODO also read contents and ensure the asset actually contains an earth and a moon
            else if (planetResult.AssetInfo != null && planetResult.AssetInfo.AssetType != GameAssetType.Level)
            {
                if (planetResult.ErrorMessage != null) dataContext.Database.AddErrorNotification("Planet update failed", "The asset was badly formatted.", user);
                return BadRequest;
            }
        }

        // Trim description
        if (data.Description is { Length: > UgcLimits.DescriptionLimit })
        {
            data.Description = data.Description[..UgcLimits.DescriptionLimit];
        }
        
        if (data.LevelLocations != null && data.LevelLocations.Count > 0)
        {
            dataContext.Database.UpdateLevelLocations(data.LevelLocations, user);
        }

        dataContext.Database.UpdateUserData(user, data, dataContext.Game);
        return OK;
    }

    private const int PinTimeoutDuration = 480;
    private const int PinRequestAmount = 8;
    private const int PinBlockDuration = 420;
    private const string PinBucket = "game-pins"; // Amount could aswell be 1, considering the default intervall in the NWS (5 minutes), but profile pin updating exists...

    [GameEndpoint("update_my_pins", HttpMethods.Post, ContentType.Json)]
    [RequireEmailVerified]
    [NullStatusCode(BadRequest)]
    [RateLimitSettings(PinTimeoutDuration, PinRequestAmount, PinBlockDuration, PinBucket)]
    public SerializedPins? UpdatePins(RequestContext context, DataContext dataContext, GameUser user, SerializedPins body, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config)) 
            return null;

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
            dataContext.Database.UpdateUserPinProgress(pinProgresses, user, dataContext.Game == TokenGame.BetaBuild, dataContext.Platform);
        }

        // Users can only have 3 pins set on their profile
        if (body.ProfilePins.Count > 3)
            return null;

        if (body.ProfilePins.Count > 0)
        {
            dataContext.Database.UpdateUserProfilePins(body.ProfilePins, user, dataContext.Game, dataContext.Platform);
        }

        // Return newly updated pins (LBP2 and 3 update their pin progresses if there are higher progress values
        // in the response, but seemingly ignore the profile pins in the response)
        return this.GetPins(context, dataContext, user);
    }

    [GameEndpoint("get_my_pins", HttpMethods.Get, ContentType.Json)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(Unauthorized)]
    [RateLimitSettings(PinTimeoutDuration, PinRequestAmount, PinBlockDuration, PinBucket)]
    public SerializedPins? GetPins(RequestContext context, DataContext dataContext, GameUser user)
    {
        return SerializedPins.FromOld
        (
            dataContext.Database.GetPinProgressesByUser(user, dataContext.Game == TokenGame.BetaBuild, dataContext.Platform, 0, 999).Items
        );
        
    }
}