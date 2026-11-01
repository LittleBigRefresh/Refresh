using System.Net;
using Microsoft.EntityFrameworkCore;
using Refresh.Common;
using Refresh.Core.Helpers;
using Refresh.Core.Types.Assets.Validation;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class ResetInvalidFaceIconsMigration : MigrationJob<GameUser>
{
    // This takes a relatively long time per user, so make the batches smaller
    protected override int BatchCount => 100;

    protected override IQueryable<GameUser> SortAndFilter(IQueryable<GameUser> query)
    {
        return query
            // Skip fake users because their face icons can't be updated currently
            // (unless the operator edits them in DB manually, but at that point we probably have bigger issues).
            // Can't use StartsWith() because it doesn't get translated to SQL apparently.
            .Where(u => !EF.Functions.ILike(u.Username, "!%"))
            .OrderBy(u => u.UserId);
    }

    protected override int Migrate(WorkContext context, GameUser[] batch)
    {
        AssetValidationParameters faceParams = new()
        {
            AssetRef = null!,
            User = null!,
            // these are only used incase it's a GUID, or for PSP stuff, but neither will happen here.
            // just set to LBP2 since that's the first game to use face icons.
            GameToUseIn = TokenGame.LittleBigPlanet2,
            PlatformToUseIn = TokenPlatform.PS3,
            Database = context.Database,
            DataStore = context.DataStore,
            AssetImporter = new(context.Logger, context.TimeProvider),
            Aipi = null,
            // the icons can legitimately be blank here, and we want to set to blank if the icon has issues here anyway
            MayBeBlank = true,
            MayBeGuid = false,
            MayBeHash = true,
            MustBeInDataStoreIfHash = true, // we have both an importer and a data store here, so why not check this too
            MustBeTexture = true,
            AssetContextTypeStr = "icon",
        };
        foreach (GameUser user in batch)
        {
            faceParams.User = user;

            // Reset yay icon if invalid
            faceParams.AssetRef = user.YayFaceHash;
            ValidatedAssetResult yayResult = ResourceValidationHelper.ValidateReference(faceParams, context.Logger);

            // an error on our side should not cause a false positive, so also ignore InternalServerError
            if (yayResult.Status != HttpStatusCode.OK && yayResult.Status != HttpStatusCode.InternalServerError)
            {
                yayResult.ErrorMessage ??= yayResult.DisallowanceInfo != null ? "The icon is disallowed" : "Unknown reason";
                context.Logger.LogInfo(RefreshContext.Worker, $"{user}'s yay face icon '{user.YayFaceHash}' will be reset because of the following reason: {yayResult.ErrorMessage}");
            }

            // Reset meh icon if invalid
            faceParams.AssetRef = user.MehFaceHash;
            ValidatedAssetResult mehResult = ResourceValidationHelper.ValidateReference(faceParams, context.Logger);

            // an error on our side should not cause a false positive, so also ignore InternalServerError
            if (mehResult.Status != HttpStatusCode.OK && mehResult.Status != HttpStatusCode.InternalServerError)
            {
                mehResult.ErrorMessage ??= mehResult.DisallowanceInfo != null ? "The icon is disallowed" : "Unknown reason";
                context.Logger.LogInfo(RefreshContext.Worker, $"{user}'s meh face icon '{user.MehFaceHash}' will be reset because of the following reason: {mehResult.ErrorMessage}");
            }

            // Reset boo icon if invalid
            faceParams.AssetRef = user.BooFaceHash;
            ValidatedAssetResult booResult = ResourceValidationHelper.ValidateReference(faceParams, context.Logger);

            // an error on our side should not cause a false positive, so also ignore InternalServerError
            if (booResult.Status != HttpStatusCode.OK && booResult.Status != HttpStatusCode.InternalServerError)
            {
                booResult.ErrorMessage ??= booResult.DisallowanceInfo != null ? "The icon is disallowed" : "Unknown reason";
                context.Logger.LogInfo(RefreshContext.Worker, $"{user}'s boo face icon '{user.BooFaceHash}' will be reset because of the following reason: {booResult.ErrorMessage}");
            }

            // Nothing to do if all icons are unchanged
            if (user.YayFaceHash == yayResult.NewAssetRef && user.MehFaceHash == mehResult.NewAssetRef && user.BooFaceHash == booResult.NewAssetRef)
            {
                context.Logger.LogDebug(RefreshContext.Worker, $"{user}'s face icons haven't changed after validation, so they will not be updated.");
                continue;
            }

            // Always save regardless
            context.Logger.LogDebug(RefreshContext.Worker, $"{user}'s face icons will be updated to: \n\t Yay '{user.YayFaceHash}' -> '{yayResult.NewAssetRef}' \n\t Meh '{user.MehFaceHash}' -> '{mehResult.NewAssetRef}' \n\t Boo '{user.BooFaceHash}' -> '{booResult.NewAssetRef}'");
            context.Database.UpdateUserFaceIcons(user, yayResult.NewAssetRef, mehResult.NewAssetRef, booResult.NewAssetRef);
        }
        return batch.Length;
    }
}