using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Common.Time;
using Refresh.Common.Verification;
using Refresh.Core;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;
using Refresh.Database;
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
    private const string BucketName = "level-publish";
    
    /// <summary>
    /// Does basic verification on a level
    /// </summary>
    /// <param name="body">The level to verify</param>
    /// <param name="dataContext">The data context associated with the request</param>
    /// <returns>Whether validation succeeded</returns>
    private static bool VerifyLevel(GameLevelRequest body, DataContext dataContext)
    {
        if (body.Title.Length > UgcLimits.TitleLimit) 
            body.Title = body.Title[..UgcLimits.TitleLimit];

        if (body.Description.Length > UgcLimits.DescriptionLimit)
            body.Description = body.Description[..UgcLimits.DescriptionLimit];
            
        if (body.MaxPlayers is > 4 or < 0 || body.MinPlayers is > 4 or < 0)
            return false;

        //If the icon hash is a GUID hash, verify that its a valid texture GUID
        if (body.IconHash.StartsWith('g') && !dataContext.GuidChecker.IsTextureGuid(dataContext.Game, long.Parse(body.IconHash.AsSpan()[1..]))) 
            return false;

        if (body.IsAdventure && dataContext.Game != TokenGame.LittleBigPlanet3)
            return false;

        GameLevel? existingLevel = dataContext.Database.GetLevelByRootResource(body.RootResource);
        // If all are true:
        // - there is an existing level with this root hash
        // - this isn't an update request
        // then block the upload
        if (existingLevel != null && body.LevelId != existingLevel.LevelId)
        {
            dataContext.Database.AddPublishFailNotification("The level you tried to publish has already been uploaded by another user.", body.Title, dataContext.User!);
            return false;
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

        return true;
    }

    private static bool IsTimedLevelLimitReached(DataContext dataContext, GameUser user, string levelTitle, TimedLevelUploadLimitProperties config, DateTimeOffset now)
    {
        if (!config.Enabled || user.TimedLevelUploads <= 0 || user.TimedLevelUploadExpiryDate == null)
        {
            return false;
        }

        DateTimeOffset expiryDate = user.TimedLevelUploadExpiryDate.Value;

        // If the expiration date has expired (less than now), reset user's limit and continue.
        if (now >= expiryDate)
        {
            dataContext.Database.ResetTimedLevelLimit(user);
            return false;
        }
        // If expiration date has not expired yet and the user has reached the limit, block.
        else if (user.TimedLevelUploads >= config.LevelQuota)
        {
            TimeSpan remainingTime = expiryDate - now;
            dataContext.Database.AddPublishFailNotification
            (
                $"You have reached the timed level upload limit of {config.LevelQuota} levels per {config.TimeSpanHours} hours. " +
                $"Your limit will expire in around {remainingTime.Hours} hours and {remainingTime.Minutes} minutes. After that, try publishing your level again!", 
                levelTitle, 
                user
            );
            return true;
        }
        else
        {
            return false;
        }
    }

    [GameEndpoint("startPublish", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    public Response StartPublish(RequestContext context,
        GameLevelRequest body,
        DataContext dataContext,
        GameServerConfig config,
        IDateTimeProvider dateTimeProvider)
    {
        if (dataContext.User!.IsWriteBlocked(config))
            return Unauthorized;
        
        if (IsTimedLevelLimitReached(dataContext, dataContext.User!, body.Title, config.TimedLevelUploadLimits, dateTimeProvider.Now)) 
            return Unauthorized;

        //If verifying the request fails, return BadRequest
        if (!VerifyLevel(body, dataContext))
        {
            context.Logger.LogInfo(RefreshContext.Publishing, "Failed to verify root level");
            return BadRequest;
        }

        if (body.Slots != null)
        {
            foreach (GameLevelRequest innerLevel in body.Slots)
            {
                if (VerifyLevel(innerLevel, dataContext)) continue;

                context.Logger.LogInfo(RefreshContext.Publishing, "Failed to verify inner level {0}", innerLevel.LevelId);
                return BadRequest;
            }
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
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response PublishLevel(RequestContext context,
        GameLevelRequest body,
        DataContext dataContext,
        GameUser user,
        GameServerConfig config,
        IDateTimeProvider dateTimeProvider)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        if (IsTimedLevelLimitReached(dataContext, user, body.Title, config.TimedLevelUploadLimits, dateTimeProvider.Now))
            return Unauthorized;

        //If verifying the request fails, return BadRequest
        if (!VerifyLevel(body, dataContext)) return BadRequest;

        string rootResourcePath = context.IsPSP() ? $"psp/{body.RootResource}" : body.RootResource;

        //Check if the root resource is a SHA1 hash
        if (!CommonPatterns.Sha1Regex().IsMatch(body.RootResource)) return BadRequest;
        //Make sure the root resource exists in the data store
        if (!dataContext.DataStore.ExistsInStore(rootResourcePath)) return NotFound;

        GameAsset? asset = dataContext.Database.GetAssetFromHash(body.RootResource);
        if (asset != null && dataContext.Game != TokenGame.LittleBigPlanetPSP)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (body.IsAdventure && asset.AssetType != GameAssetType.AdventureCreateProfile)
            {
                dataContext.Database.AddPublishFailNotification("The uploaded adventure data was corrupted.", body.Title, dataContext.User!);
                return BadRequest;
            }

            if (!body.IsAdventure && asset.AssetType != GameAssetType.Level)
            {
                dataContext.Database.AddPublishFailNotification("The uploaded level data was corrupted.", body.Title, dataContext.User!);
                return BadRequest;
            }
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

            bool isFullEdit = body.RootResource != levelToUpdate.RootResource;
            levelToUpdate = dataContext.Database.UpdateLevel(body, levelToUpdate, dataContext.Game);

            if (isFullEdit)
            {
                // If the level's root resource was edited, update the modded status aswell.
                // The NOTE from below applies here aswell.
                dataContext.Database.UpdateLevelModdedStatus(levelToUpdate);
            }

            dataContext.Database.UpdateSkillRewardsForLevel(levelToUpdate, body.SkillRewards);
            return new Response(GameLevelResponse.FromOld(levelToUpdate, dataContext)!, ContentType.Xml);
        }

        GameLevel newLevel = dataContext.Database.AddLevel(body, dataContext.Game, user);
        dataContext.Database.UpdateSkillRewardsForLevel(newLevel, body.SkillRewards);

        context.Logger.LogInfo(BunkumCategory.UserContent, "User {0} (id: {1}) uploaded level id {2}", user.Username, user.UserId, newLevel.LevelId);

        // Only increment if the level can be uploaded (right after the previous checks + adding the level),
        // don't want to increment for failed uploads
        if (config.TimedLevelUploadLimits.Enabled)
        {
            dataContext.Database.IncrementTimedLevelLimit(user, config.TimedLevelUploadLimits.TimeSpanHours);
        }

        // Update the modded status of the level
        // NOTE: this wont do anything if the slot is uploaded before the level resource,
        //       so we also do this same operation inside of ResourceEndpoints.UploadAsset to catch that case aswell
        dataContext.Database.UpdateLevelModdedStatus(newLevel);
        dataContext.Database.CreateLevelUploadEvent(user, newLevel);

        return new Response(GameLevelResponse.FromOld(newLevel, dataContext)!, ContentType.Xml);
    }

    [GameEndpoint("unpublish/{id}", ContentType.Xml, HttpMethods.Post)]
    public Response DeleteLevel(RequestContext context, GameUser user, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        if (level.Publisher?.UserId != user.UserId) return Unauthorized;

        database.DeleteLevel(level);
        return OK;
    }
}