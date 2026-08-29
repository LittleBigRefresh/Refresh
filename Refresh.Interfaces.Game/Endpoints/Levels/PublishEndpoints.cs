using System.Net;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common;
using Refresh.Common.Constants;
using Refresh.Common.Time;
using Refresh.Common.Verification;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Helpers;
using Refresh.Core.Importing;
using Refresh.Core.Services;
using Refresh.Core.Types.Assets.Validation;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Request;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Levels;

namespace Refresh.Interfaces.Game.Endpoints.Levels;

public class PublishEndpoints : EndpointGroup
{
    private const int RequestTimeoutDuration = 900; // 15 minutes
    private const int MaxRequestAmount = 15;
    private const int RequestBlockDuration = RequestTimeoutDuration;
    private const string PublishBucket = "level-publish";
    private const string StartPublishBucket = "level-start-publish";
    
    /// <summary>
    /// Does basic verification on a level
    /// </summary>
    /// <param name="body">The level to verify</param>
    /// <param name="dataContext">The data context associated with the request</param>
    /// <returns>Whether validation succeeded. OK = success; everything else = failure</returns>
    private static HttpStatusCode VerifyLevel(GameLevelRequest body, DataContext dataContext, AssetImporter importer, AipiService aipi, bool isActualPublish, bool isInnerLevel = false)
    {
        if (body.Title.Length > UgcLimits.TitleLimit) 
            body.Title = body.Title[..UgcLimits.TitleLimit];

        if (body.Description.Length > UgcLimits.DescriptionLimit)
            body.Description = body.Description[..UgcLimits.DescriptionLimit];
            
        if (body.MaxPlayers is > 4 or < 0 || body.MinPlayers is > 4 or < 0)
        {
            dataContext.Database.AddPublishFailNotification("Your player number restrictions were invalid.", body.Title, dataContext.User!);
            return BadRequest;
        }

        if (body.IsAdventure && isInnerLevel)
        {
            dataContext.Database.AddPublishFailNotification("An adventure may not include inner adventures.", body.Title, dataContext.User!);
            return BadRequest;
        }

        if (body.IsAdventure && dataContext.Game != TokenGame.LittleBigPlanet3 && dataContext.Game != TokenGame.BetaBuild)
        {
            dataContext.Database.AddPublishFailNotification("You may only publish adventures in LBP3 or beta builds.", body.Title, dataContext.User!);
            return Unauthorized;
        }

        if (!body.IsAdventure && body.Slots != null && body.Slots.Length > 0)
        {
            dataContext.Database.AddPublishFailNotification("Only adventures may include inner levels.", body.Title, dataContext.User!);
            return BadRequest;
        }

        // Validate icon
        AssetValidationParameters iconParams = new(body.IconHash, dataContext, importer, aipi)
        {
            MustBeTexture = true,
            MustBeInDataStoreIfHash = isActualPublish, // in most cases no assets will be uploaded yet when startPublish is called
            AssetContextTypeStr = "icon",
        };
        ValidatedAssetResult iconResult = ResourceValidationHelper.ValidateReference(iconParams, dataContext.Logger);
        body.IconHash = iconResult.NewAssetRef;
        if (iconResult.Status != OK)
        {
            if (iconResult.ErrorMessage != null) dataContext.Database.AddPublishFailNotification(iconResult.ErrorMessage, body.Title, dataContext.User!);
            return iconResult.Status;
        }
        
        // For some stupid reason, inner adventure levels don't include their root hash in the request.
        // TODO: validate their root hash once we finally start to read them from the adventure root asset itself, as it is included there.
        if (!isInnerLevel)
        {
            // Validate root resource
            AssetValidationParameters rootParams = new(body.RootResource, dataContext, importer)
            {
                MayBeBlank = false,
                MayBeGuid = false,
                MustBeInDataStoreIfHash = isActualPublish, // in most cases no assets will be uploaded yet when startPublish is called
                AssetContextTypeStr = body.IsAdventure ? "adventure asset" : "level asset",
            };
            ValidatedAssetResult rootResult = ResourceValidationHelper.ValidateReference(rootParams, dataContext.Logger);
            body.RootResource = rootResult.NewAssetRef;
            if (rootResult.Status != OK)
            {
                if (rootResult.ErrorMessage != null) dataContext.Database.AddPublishFailNotification(rootResult.ErrorMessage, body.Title, dataContext.User!);
                return rootResult.Status;
            }
            else if (rootResult.AssetInfo != null && dataContext.Game != TokenGame.LittleBigPlanetPSP) // PSP uses a completely different asset type which we don't validate yet
            {
                if (body.IsAdventure && rootResult.AssetInfo.AssetType != GameAssetType.AdventureCreateProfile)
                {
                    if (rootResult.ErrorMessage != null) dataContext.Database.AddPublishFailNotification("The adventure asset was badly formatted.", body.Title, dataContext.User!);
                    return BadRequest;
                }
                if (!body.IsAdventure && rootResult.AssetInfo.AssetType != GameAssetType.Level)
                {
                    if (rootResult.ErrorMessage != null) dataContext.Database.AddPublishFailNotification("The level asset was badly formatted.", body.Title, dataContext.User!);
                    return BadRequest;
                }
            }

            GameLevel? existingLevel = dataContext.Database.GetLevelByRootResource(body.RootResource);
            // If all are true:
            // - there is an existing level with this root hash
            // - this isn't an update request
            // then block the upload
            if (existingLevel != null && body.LevelId != existingLevel.LevelId)
            {
                dataContext.Database.AddPublishFailNotification("The level you tried to publish has already been uploaded by another user.", body.Title, dataContext.User!);
                return Unauthorized;
            }
        }

        if (!string.IsNullOrWhiteSpace(body.PublisherLabels))
        {
            // Unknown labels will be ignored and be missing from the level. No notifications needed because the publisher should
            // immediately be able to tell if there are missing labels on the level they just published.
            // Also make sure there are no duplicate labels submitted.
            body.FinalPublisherLabels = LabelExtensions.FromLbpCommaList(body.PublisherLabels)
                .Distinct()
                .Take(UgcLimits.MaximumLabels);
        }

        if (body.Slots != null)
        {
            foreach (GameLevelRequest innerLevel in body.Slots)
            {
                HttpStatusCode innerValidationResult = VerifyLevel(innerLevel, dataContext, importer, aipi, isActualPublish, true);
                if (innerValidationResult == OK) continue;

                dataContext.Logger.LogInfo(RefreshContext.Publishing, "Failed to verify inner level {0}", innerLevel.LevelId);
                return innerValidationResult;
            }
        }

        return OK;
    }

    private static bool IsTimedLevelLimitReached(DataContext dataContext, GameUser user, string levelTitle, EntityUploadRateLimitProperties levelLimit)
    {
        if (!levelLimit.Enabled) return false;

        TimeSpan? rateLimitExpiresIn = dataContext.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Level, levelLimit.UploadQuota);
        if (rateLimitExpiresIn != null)
        {
            dataContext.Database.AddPublishFailNotification
            (
                $"You have published too many levels recently! Your limit is {levelLimit.UploadQuota} levels per {levelLimit.TimeSpanHours} hours. " +
                $"Try again in {rateLimitExpiresIn.Value.Hours} hours and {rateLimitExpiresIn.Value.Minutes} minutes.", 
                levelTitle,
                user
            );
            return true;
        }
        
        return false;
    }

    [GameEndpoint("startPublish", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, StartPublishBucket)]
    public Response StartPublish(RequestContext context,
        GameLevelRequest body,
        DataContext dataContext,
        GameServerConfig config,
        AssetImporter importer,
        AipiService aipi,
        GameUser user)
    {
        if (dataContext.User!.IsWriteBlocked(config))
        {
            dataContext.Database.AddPublishFailNotification("The server is in read-only mode.", body.Title, dataContext.User!);
            return Unauthorized;
        }
        
        if (IsTimedLevelLimitReached(dataContext, dataContext.User!, body.Title, user.GetRolePermissionsForUser(config).LevelUploadRateLimit)) 
            return Unauthorized;

        HttpStatusCode validationResult = VerifyLevel(body, dataContext, importer, aipi, false);
        if (validationResult != OK)
        {
            context.Logger.LogInfo(RefreshContext.Publishing, "Failed to verify level");
            return validationResult;
        }

        List<string> hashes =
        [
            ..body.XmlResources,
            body.RootResource,
            body.IconHash,
        ];

        //Remove all invalid or GUID assets
        hashes.RemoveAll(r => r == "0" || r.StartsWith('g') || string.IsNullOrWhiteSpace(r));

        //Verify all hashes are valid SHA1 hashes
        if (hashes.Any(hash => !CommonPatterns.Sha1Regex().IsMatch(hash))) return BadRequest;

        SerializedLevelResources response = new()
        {
            Resources = hashes.Where(r => !dataContext.DataStore.ExistsInStore(r)).ToArray()
        };

        return new Response(response, ContentType.Xml);
    }

    [GameEndpoint("publish", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, PublishBucket)]
    public Response PublishLevel(RequestContext context,
        GameLevelRequest body,
        DataContext dataContext,
        GameUser user,
        AssetImporter importer,
        AipiService aipi,
        GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        EntityUploadRateLimitProperties timedLevelLimit = user.GetRolePermissionsForUser(config).LevelUploadRateLimit;
        if (IsTimedLevelLimitReached(dataContext, user, body.Title, timedLevelLimit))
            return Unauthorized;

        HttpStatusCode validationResult = VerifyLevel(body, dataContext, importer, aipi, true);
        if (validationResult != OK)
        {
            context.Logger.LogInfo(RefreshContext.Publishing, "Failed to verify level");
            return validationResult;
        }

        if (body.LevelId != 0) // Republish requests contain the id of the old level
        {
            context.Logger.LogInfo(BunkumCategory.UserContent, "Republishing level id {0}", body.LevelId);

            GameLevel? levelToUpdate = dataContext.Database.GetLevelById(body.LevelId);
            if (levelToUpdate == null) return NotFound;

            // Don't let users edit levels by others
            if (levelToUpdate.Publisher != user)
            {
                dataContext.Database.AddPublishFailNotification("You may not republish another user's level.", body.Title, dataContext.User!);
                return BadRequest;
            }

            // LBP3 doesn't allow users to overwrite LBP1/2 levels, however editing their metadata will cause LBP3 to reserialize the root resource,
            // upload it and put its hash into the republish request body, making the levels unplayable in LBP1/2.
            if (dataContext.Game == TokenGame.LittleBigPlanet3 && levelToUpdate.GameVersion != TokenGame.LittleBigPlanet3)
            {
                body.RootResource = levelToUpdate.RootResource;
            }
            
            bool isFullEdit = body.RootResource != levelToUpdate.RootResource;
            levelToUpdate = dataContext.Database.UpdateLevel(body, levelToUpdate, dataContext.Game);

            if (isFullEdit)
            {
                // If the level's root resource was edited, update the modded status aswell.
                // The NOTE from below applies here aswell.
                dataContext.Database.UpdateLevelModdedStatus(levelToUpdate);
            }

            List<GameSkillReward> updatedRewards = dataContext.Database.UpdateSkillRewardsForLevel(levelToUpdate, body.SkillRewards);
            return new Response(GameLevelResponse.FromOld(levelToUpdate, dataContext)!, ContentType.Xml);
        }

        GameLevel newLevel = dataContext.Database.AddLevel(body, dataContext.Game, user);
        List<GameSkillReward> newRewards = dataContext.Database.UpdateSkillRewardsForLevel(newLevel, body.SkillRewards);

        context.Logger.LogInfo(BunkumCategory.UserContent, "User {0} (id: {1}) uploaded level id {2}", user.Username, user.UserId, newLevel.LevelId);

        // Only increment if the level can be uploaded (right after the previous checks + adding the level),
        // don't want to increment for failed uploads
        if (timedLevelLimit.Enabled)
        {
            dataContext.Database.IncrementUploadRateLimitForEntity(user, GameDatabaseEntity.Level, timedLevelLimit.TimeSpanHours);
        }

        // Update the modded status of the level
        // NOTE: this wont do anything if the slot is uploaded before the level resource,
        //       so we also do this same operation inside of ResourceEndpoints.UploadAsset to catch that case aswell
        dataContext.Database.UpdateLevelModdedStatus(newLevel);
        dataContext.Database.CreateLevelUploadEvent(user, newLevel);

        return new Response(GameLevelResponse.FromOld(newLevel, dataContext)!, ContentType.Xml);
    }

    [GameEndpoint("unpublish/{id}", ContentType.Xml, HttpMethods.Post)]
    public Response DeleteLevel(RequestContext context, GameUser user, GameDatabaseContext database, int id, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        if (level.Publisher?.UserId != user.UserId) return Unauthorized;

        database.DeleteLevel(level);
        return OK;
    }
}