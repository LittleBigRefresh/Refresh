using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Helpers;
using Refresh.Core.Importing;
using Refresh.Core.RateLimits.Photos;
using Refresh.Core.Services;
using Refresh.Core.Types.Assets.Validation;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Endpoints;

public class PhotoEndpoints : EndpointGroup
{
    [GameEndpoint("uploadPhoto", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    [RateLimitSettings(300, 30, 240, "upload-photo")]
    public Response UploadPhoto(RequestContext context, SerializedPhoto body, GameDatabaseContext database,
        GameUser user, IDataStore dataStore, AssetImporter importer,
        DataContext dataContext, AipiService aipi, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        EntityUploadRateLimitProperties uploadLimit = user.GetRolePermissionsForUser(config).PhotoUploadRateLimit;
        if (uploadLimit.Enabled)
        {
            TimeSpan? rateLimitExpiresIn = database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Photo, uploadLimit.UploadQuota);
            if (rateLimitExpiresIn != null)
            {
                dataContext.Database.AddErrorNotification
                (
                    "Photo upload failed",
                    $"You have uploaded too many photos recently! Your limit is {uploadLimit.UploadQuota} photos per {uploadLimit.TimeSpanHours} hours. " +
                    $"Try again in {rateLimitExpiresIn.Value.Hours} hours and {rateLimitExpiresIn.Value.Minutes} minutes.", 
                    user
                );
                return Unauthorized;
            }
        }

        if (body.PhotoSubjects.Count > 4)
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, $"Too many subjects in photo, rejecting photo upload. Uploader: {user.UserId}");
            database.AddErrorNotification("Photo upload failed", "The photo had more than 4 players", user);
            return BadRequest;
        }

        if (body.PhotoSubjects.Any(s => s.Username.Equals("LBPMod.me", StringComparison.InvariantCultureIgnoreCase)))
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, $"Photo contains disallowed subjects, rejecting photo upload. Uploader: {user.UserId}");
            return Unauthorized;
        }

        GamePhoto? existingPhoto = database.GetPhotoByAnyHash(body.SmallHash, body.MediumHash, body.LargeHash, body.PlanHash);
        if (existingPhoto != null)
        {
            // TODO: show photo names in these error notifications once we start to deserialize and store plan data
            database.AddErrorNotification("Photo upload failed", "The photo already exists on the server", user);
            return BadRequest;
        }

        // Plan
        // TODO validate content
        AssetValidationParameters planParams = new(body.PlanHash, dataContext, importer)
        {
            MayBeBlank = false,
            MayBeGuid = false,
            AssetContextTypeStr = "metadata asset",
        };
        ValidatedAssetResult planResult = ResourceValidationHelper.ValidateReference(planParams, context.Logger);
        body.PlanHash = planResult.NewAssetRef;

        if (planResult.Status != OK)
        {
            if (planResult.ErrorMessage != null) database.AddErrorNotification("Photo upload failed", planResult.ErrorMessage, user);
            return planResult.Status;
        }
        // should never be null at this point, but let's just be extra safe
        else if (planResult.AssetInfo != null && planResult.AssetInfo.AssetType != GameAssetType.Plan)
        {
            database.AddErrorNotification("Photo upload failed", "The metadata asset was badly formatted.", user);
            return BadRequest;
        }

        // Don't just iterate the images because we might eventually want to go further and also validate each format's type
        // (e.g. for mainline, small and medium are TEX, while large is JPEG), see https://github.com/LittleBigRefresh/Refresh/issues/977
        // Small image
        AssetValidationParameters imageParams = new(body.SmallHash, dataContext, importer, aipi)
        {
            MayBeBlank = false,
            MayBeGuid = false,
            MustBeTexture = true,
            AssetContextTypeStr = "image",
        };
        ValidatedAssetResult imageResult = ResourceValidationHelper.ValidateReference(imageParams, context.Logger);
        body.SmallHash = imageResult.NewAssetRef;

        if (imageResult.Status != OK)
        {
            if (imageResult.ErrorMessage != null) database.AddErrorNotification("Photo upload failed", imageResult.ErrorMessage, user);
            return imageResult.Status;
        }

        // Medium image
        imageParams.AssetRef = body.MediumHash;
        imageResult = ResourceValidationHelper.ValidateReference(imageParams, context.Logger);
        body.MediumHash = imageResult.NewAssetRef;

        if (imageResult.Status != OK)
        {
            if (imageResult.ErrorMessage != null) database.AddErrorNotification("Photo upload failed", imageResult.ErrorMessage, user);
            return imageResult.Status;
        }

        // Large image
        imageParams.AssetRef = body.LargeHash;
        imageResult = ResourceValidationHelper.ValidateReference(imageParams, context.Logger);
        body.LargeHash = imageResult.NewAssetRef;

        if (imageResult.Status != OK)
        {
            if (imageResult.ErrorMessage != null) database.AddErrorNotification("Photo upload failed", imageResult.ErrorMessage, user);
            return imageResult.Status;
        }

        GameLevel? level = body.Level == null ? null : database.GetLevelByIdAndType(body.Level.Type, body.Level.LevelId);

        database.UploadPhoto(body, body.PhotoSubjects, user, level);
        
        if (uploadLimit.Enabled)
        {
            database.IncrementUploadRateLimitForEntity(user, GameDatabaseEntity.Photo, uploadLimit.TimeSpanHours);
        }

        return OK;
    }

    [GameEndpoint("deletePhoto/{id}", HttpMethods.Post)]
    public Response DeletePhoto(RequestContext context, GameDatabaseContext database, GameUser user, int id, DataContext dataContext)
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
        IEnumerable<SerializedPhoto> photos = photoGetter.Invoke(user, count, skip)
            .Items
            .ToArray()
            .Select(photo => PhotoExtensions.FromGamePhoto(photo, dataContext));

        return new Response(new SerializedPhotoList(photos), ContentType.Xml);
    }

    [GameEndpoint("photos/with", ContentType.Xml)]
    [Authentication(false)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(PhotoListEndpointLimits.TimeoutDuration, PhotoListEndpointLimits.GameRequestAmount, 
                            PhotoListEndpointLimits.BlockDuration, PhotoListEndpointLimits.GameRequestBucket)]
    public Response PhotosWithUser(RequestContext context, GameDatabaseContext database, DataContext dataContext) 
        => GetPhotos(context, database, dataContext, database.GetPhotosWithUser);
    
    [GameEndpoint("photos/by", ContentType.Xml)]
    [Authentication(false)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(PhotoListEndpointLimits.TimeoutDuration, PhotoListEndpointLimits.GameRequestAmount, 
                            PhotoListEndpointLimits.BlockDuration, PhotoListEndpointLimits.GameRequestBucket)]
    public Response PhotosByUser(RequestContext context, GameDatabaseContext database, DataContext dataContext) 
        => GetPhotos(context, database, dataContext, database.GetPhotosByUser);

    [GameEndpoint("photos/{slotType}/{levelId}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(PhotoListEndpointLimits.TimeoutDuration, PhotoListEndpointLimits.GameRequestAmount, 
                            PhotoListEndpointLimits.BlockDuration, PhotoListEndpointLimits.GameRequestBucket)]
    public SerializedPhotoList? GetPhotosOnLevel(RequestContext context, DataContext dataContext, string slotType, int levelId)
    {
        GameLevel? level = dataContext.Database.GetLevelByIdAndType(slotType, levelId);
        if (level == null) 
            return null;

        (int skip, int count) = context.GetPageData();
        
        // if the game specifies whose photos to get which are uploaded for this level
        string? username = context.QueryString.Get("by");
        GameUser? user = dataContext.Database.GetUserByUsername(username);
        
        DatabaseList<GamePhoto> photos;
        
        if (user != null)
            photos = dataContext.Database.GetPhotosInLevelByUser(level, user, count, skip);
        else
            photos = dataContext.Database.GetPhotosInLevel(level, count, skip);

        // count not used ingame
        return new SerializedPhotoList(photos.Items.ToArrayIfPostgres().Select(photo => PhotoExtensions.FromGamePhoto(photo, dataContext)));
    }

    [GameEndpoint("photo/{id}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(SinglePhotoEndpointLimits.TimeoutDuration, SinglePhotoEndpointLimits.RequestAmount, 
                            SinglePhotoEndpointLimits.BlockDuration, SinglePhotoEndpointLimits.RequestBucket)]
    public SerializedPhoto? GetPhotoById(RequestContext context, DataContext dataContext, int id)
    {
        GamePhoto? photo = dataContext.Database.GetPhotoById(id);

        if (photo == null) 
            return null;
        
        return PhotoExtensions.FromGamePhoto(photo, dataContext);
    }
}