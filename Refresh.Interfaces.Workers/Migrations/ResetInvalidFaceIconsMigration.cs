using System.Net;
using Microsoft.EntityFrameworkCore;
using NotEnoughLogs;
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
        List<NewFaceHashes> pendingUpdates = [];

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

            // Validate the hashes
            NewFaceHashes newHashes = new()
            {
                User = user,
                YayHash = this.ValidateIcon(faceParams, user, user.YayFaceHash, "Yay", context.Logger),
                MehHash = this.ValidateIcon(faceParams, user, user.MehFaceHash, "Meh", context.Logger),
                BooHash = this.ValidateIcon(faceParams, user, user.BooFaceHash, "Boo", context.Logger),
            };

            // Nothing to do if all icons are unchanged
            if (user.YayFaceHash == newHashes.YayHash && user.MehFaceHash == newHashes.MehHash && user.BooFaceHash == newHashes.BooHash)
            {
                context.Logger.LogDebug(RefreshContext.Worker, $"{user}'s face icons haven't changed after validation, so they will not be updated.");
                continue;
            }

            // Else queue the user and their new hashes to the bulk update after this loop
            pendingUpdates.Add(newHashes);

            context.Logger.LogDebug(RefreshContext.Worker, $"{user}'s face icons will be updated to:" +
                $" \n\t Yay '{user.YayFaceHash}' -> '{newHashes.YayHash}' \n\t Meh '{user.MehFaceHash}' -> '{newHashes.MehHash}' \n\t Boo '{user.BooFaceHash}' -> '{newHashes.BooHash}'");
        }

        // Save separately in bulks, instead of individually sending writes for every user in the loop above,
        // otherwise this will make work cycles slower. (This has been a major bottleneck in my testing).
        foreach (NewFaceHashes hashes in pendingUpdates)
        {
            context.Database.UpdateUserFaceIcons(hashes.User, hashes.YayHash, hashes.MehHash, hashes.BooHash, false);
        }

        context.Database.SaveChanges();
        return batch.Length;
    }

    private string ValidateIcon(AssetValidationParameters parameters, GameUser user, string hash, string name, Logger logger)
    {
        parameters.AssetRef = hash;
        parameters.AssetContextTypeStr = $"{name} face icon";
        parameters.User = user; // just in case

        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(parameters, logger);

        // An error on our side should not cause a false positive, so also treat InternalServerError as success.
        // Better than resetting many face icons because of issues on the operator's end, instead of the icons actually being invalid.
        if (result.Status == HttpStatusCode.OK || result.Status == HttpStatusCode.InternalServerError)
        {
            // No need to log anything in this case.
            // Don't just return the hash param value, incase e.g. it's a blank hash which we want to normalize to 0.
            return result.NewAssetRef;
        }

        result.ErrorMessage ??= result.DisallowanceInfo != null ? "The icon is disallowed" : "Unknown reason";
        logger.LogInfo(RefreshContext.Worker, $"{user}'s {name} face icon '{hash}' will be reset because of the following reason: {result.ErrorMessage}");

        // At this point result.NewAssetRef should always already be 0, but incase it's somehow not, return 0 explicitly,
        // since we've already told the operator that we're going to reset the icon.
        return "0";
    }

    internal struct NewFaceHashes
    {
        public GameUser User;
        public string YayHash;
        public string MehHash;
        public string BooHash;
    }
}