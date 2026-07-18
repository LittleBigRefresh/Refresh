using System.Diagnostics;
using Bunkum.Core;
using NotEnoughLogs;
using Refresh.Common.Verification;
using Refresh.Core.Types.Assets.Validation;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;

namespace Refresh.Core.Helpers;

public abstract class ResourceValidationHelper
{
    /// <summary>
    /// Validates the given asset reference (hash/guid/blank) using the given parameters, if necessary also by reading the referenced asset
    /// or getting data about it from database (see <see cref="GameAsset"/> and <see cref="DisallowedAsset"/>).
    /// </summary>
    public static ValidatedAssetResult ValidateReference(AssetValidationParameters parameters, Logger logger)
    {
        string assetTypeStr = parameters.AssetContextTypeStr ?? (parameters.MustBeTexture ? "image asset" : "asset");
        GameAsset? asset = null;
        bool existsInDataStore = false;
        bool isPSP = parameters.GameToUseIn == TokenGame.LittleBigPlanetPSP;

        if (parameters.AssetRef.IsBlankHash())
        {
            if (!parameters.MayBeBlank) return new(BadRequest, "0", $"The {assetTypeStr} must be set.");
            else return new(OK, "0", null);
        }

        else if (parameters.AssetRef.StartsWith('g'))
        {
            if (!parameters.MayBeGuid) return new(BadRequest, null, $"The {assetTypeStr} may not be an in-game asset.");
            if (parameters.AssetRef.Length < 2) return new(BadRequest, null, $"The used in-game {assetTypeStr} is invalid (empty GUID).");

            // This should only happen if the user is messing with mods/the API/beta builds, so give them a more detailed response
            bool canParseGuid = long.TryParse(parameters.AssetRef[1..], out long guid);
            if (!canParseGuid)
                return new(BadRequest, null, $"The used in-game {assetTypeStr} is invalid (badly formatted GUID).");

            if (parameters.MustBeTexture && !parameters.GuidChecker.IsTextureGuid(parameters.GameToUseIn, guid))
                return new(BadRequest, null, $"The used in-game {assetTypeStr} was not a valid image (unknown GUID).");
        }

        // At this point the reference is a hash
        else if (!parameters.MayBeHash)
        {
            return new(BadRequest, null, $"The {assetTypeStr} may not be a custom asset.");
        }

        else if (!CommonPatterns.Sha1Regex().IsMatch(parameters.AssetRef))
        {
            // This should only happen if a player is messing with mods/the API, so give them a more detailed response
            return new(BadRequest, null, $"The used {assetTypeStr} had an invalid hash.");
        }

        else
        {
            DisallowedAsset? disallowed = parameters.Database.GetDisallowedAssetInfo(parameters.AssetRef);
            if (disallowed != null)
            {
                logger.LogWarning(BunkumCategory.UserContent, $"{parameters.User} tried to use a manually disallowed {assetTypeStr}.");
                return new(Unauthorized, disallowanceInfo: disallowed);
            }

            string filename = isPSP ? $"psp/{parameters.AssetRef}" : parameters.AssetRef;
            existsInDataStore = parameters.DataStore.ExistsInStore(filename);

            if (!existsInDataStore)
            {
                logger.LogDebug(BunkumCategory.UserContent, $"Referenced asset '{filename}' could not be found in data store.");

                if (parameters.MustBeInDataStoreIfHash)
                    return new(NotFound, null, $"The used {assetTypeStr} did not exist on the server.");
            }

            asset = parameters.Cache.GetAssetInfo(parameters.AssetRef, parameters.Database);

            // Only try to import if the asset exists in the data store
            if (existsInDataStore && asset == null)
            {
                logger.LogInfo(BunkumCategory.UserContent, $"Referenced asset '{filename}' exists in data store but not in database, attempting to import automatically...");
                Stopwatch sw = new();
                sw.Start();

                if (!parameters.DataStore.TryGetDataFromStore(filename, out byte[]? assetData) || assetData == null)
                {
                    sw.Stop();
                    logger.LogError(BunkumCategory.UserContent, $"Failed to read '{filename}' from data store!");
                    logger.LogDebug(BunkumCategory.UserContent, $"Failed to get '{filename}' after {sw.ElapsedMilliseconds}ms.");
                    return new(InternalServerError, null, $"Failed to read {assetTypeStr} internally. Please report this to the server owner.", existsInDataStore: existsInDataStore);
                }

                asset = parameters.AssetImporter.ReadAndVerifyAsset(parameters.AssetRef, assetData, parameters.PlatformToUseIn, parameters.Database);
                if (asset == null) 
                {
                    sw.Stop();
                    logger.LogDebug(BunkumCategory.UserContent, $"Failed to get '{filename}' after {sw.ElapsedMilliseconds}ms.");
                    return new(BadRequest, null, $"The used {assetTypeStr} was invalid or corrupt.", existsInDataStore: existsInDataStore);
                }

                sw.Stop();
                logger.LogInfo(BunkumCategory.UserContent, $"Successfully imported '{filename}' in {sw.ElapsedMilliseconds}ms.");
            }

            // FIXME: for some reason, PSP texture detection/conversion broke so we can no longer tell if a PSP texture is actually a texture, so skip this for PSP
            if (asset != null && !isPSP)
            {
                bool isHashedTexture = (asset.AssetFlags & AssetFlags.Imagery) != 0;

                if (parameters.MustBeTexture && !isHashedTexture)
                    return new(BadRequest, null, $"The used {assetTypeStr} was not a valid custom image.", assetInfo: asset, existsInDataStore: existsInDataStore);
                
                // TODO: actually use AIPI to scan image if not null
            }
        }

        return new(OK, parameters.AssetRef, assetInfo: asset, existsInDataStore: existsInDataStore);
    }
}