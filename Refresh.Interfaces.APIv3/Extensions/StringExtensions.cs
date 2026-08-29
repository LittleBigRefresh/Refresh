using Refresh.Core.Types.Data;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;

namespace Refresh.Interfaces.APIv3.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Validates the given icon reference (may be null, blank, a GUID or a hash).
    /// Returns the icon reference to use, and an ApiError if validation failed.
    /// </summary>
    public static (string?, ApiError?) ValidateIcon(this string? iconReference, DataContext dataContext)
    {
        if (iconReference == null) 
            return (null, null);

        else if (iconReference.IsBlankHash())
            return ("0", null);

        else if (iconReference.StartsWith('g') && iconReference.Length > 1)
        {
            bool isGuid = long.TryParse(iconReference[1..], out long guid);
            if (!isGuid || (isGuid && !dataContext.GuidChecker.IsTextureGuid(TokenGame.LittleBigPlanet1, guid)))
                return (null, ApiValidationError.InvalidTextureGuidError);
        }
        else
        {
            GameAsset? asset = dataContext.Database.GetAssetFromHash(iconReference);

            if (asset == null) 
                return (null, ApiValidationError.IconMissingError);

            if (asset.AssetType is not GameAssetType.Jpeg and not GameAssetType.Png)
                return (null, ApiValidationError.IconMustBeImageError);
        }

        return (iconReference, null);
    }
}
