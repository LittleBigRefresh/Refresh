using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Moderation;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Disallowed;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminDisallowanceApiEndpoints : EndpointGroup
{
    // TODO: use mod log
    // TODO: update disallowance rows if attempted to insert again

    #region Asset hashes

    [ApiV3Endpoint("admin/disallowed/assetHashes"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets a paginated list of disallowed asset hashes, optionally filtered by asset type.")]
    [DocError(typeof(ApiValidationError), "The asset type couldn't be parsed")]
    public ApiListResponse<ApiDisallowedAssetResponse> GetDisallowedAssetHashes(RequestContext context, DataContext dataContext, GameUser user)
    {
        (int skip, int count) = context.GetPageData();

        GameAssetType? type = null;
        string? typeParam = context.QueryString.Get("type");
        if (typeParam != null)
        {
            bool parsed = Enum.TryParse(typeParam, true, out GameAssetType typeParsed);
            if (!parsed)
            {
                return new ApiValidationError($"The asset type '{typeParam}' couldn't be parsed. Possible values: "
                    + string.Join(", ", Enum.GetNames(typeof(GameAssetType))));
            }

            type = typeParsed;
        }

        DatabaseList<DisallowedAsset> Assets = dataContext.Database.GetDisallowedAssets(type, skip, count);
        return DatabaseListExtensions.FromOldList<ApiDisallowedAssetResponse, DisallowedAsset>(Assets, dataContext);
    }

    [ApiV3Endpoint("admin/disallowed/assetHashes/hash/{hash}", HttpMethods.Post), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Adds an asset hash (with optional asset type) to the list of disallowed hashes.")]
    [DocError(typeof(ApiValidationError), "The asset type couldn't be parsed")]
    public ApiResponse<ApiDisallowedAssetResponse> DisallowAssetHash(RequestContext context, DataContext dataContext, GameUser user,
        string hash, ApiDisallowAssetRequest body)
    {
        GameAssetType type = GameAssetType.Unknown;
        if (body.Type != null)
        {
            bool parsed = Enum.TryParse(body.Type, true, out GameAssetType typeParsed);
            if (!parsed)
            {
                return new ApiValidationError($"The asset type '{body.Type}' couldn't be parsed. Possible values: "
                    + string.Join(", ", Enum.GetNames(typeof(GameAssetType))));
            }

            type = typeParsed;
        }

        (DisallowedAsset disallowed, bool success) = dataContext.Database.DisallowAsset(hash, type, body.Reason ?? "");
        return new(ApiDisallowedAssetResponse.FromOld(disallowed, dataContext)!, success ? Created : OK);
    }

    [ApiV3Endpoint("admin/disallowed/assetHashes/hash/{hash}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Removes an asset hash from the list of disallowed hashes.")]
    public ApiOkResponse ReallowAssetHash(RequestContext context, DataContext dataContext, GameUser user,
        string hash)
    {
        dataContext.Database.ReallowAsset(hash);
        return new ApiOkResponse();
    }

    #endregion
    #region Usernames

    [ApiV3Endpoint("admin/disallowed/usernames"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets a paginated list of disallowed usernames.")]
    public ApiListResponse<ApiDisallowedUsernameResponse> GetDisallowedUsernames(RequestContext context, DataContext dataContext, GameUser user)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<DisallowedUser> Usernames = dataContext.Database.GetDisallowedUsers(skip, count);
        return DatabaseListExtensions.FromOldList<ApiDisallowedUsernameResponse, DisallowedUser>(Usernames, dataContext);
    }

    [ApiV3Endpoint("admin/disallowed/usernames/name/{username}", HttpMethods.Post), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Adds a username to the list of disallowed names.")]
    public ApiResponse<ApiDisallowedUsernameResponse> DisallowUsername(RequestContext context, DataContext dataContext, GameUser user,
        string username, ApiModerationRequest body)
    {
        (DisallowedUser disallowed, bool success) = dataContext.Database.DisallowUser(username, body.Reason ?? "");
        return new(ApiDisallowedUsernameResponse.FromOld(disallowed, dataContext)!, success ? Created : OK);
    }

    [ApiV3Endpoint("admin/disallowed/usernames/name/{username}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Removes a username from the list of disallowed names.")]
    public ApiOkResponse ReallowUsername(RequestContext context, DataContext dataContext, GameUser user,
        string username)
    {
        dataContext.Database.ReallowUser(username);
        return new ApiOkResponse();
    }

    #endregion
    #region Email addresses

    [ApiV3Endpoint("admin/disallowed/emailAddresses"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets a paginated list of disallowed email addresses.")]
    public ApiListResponse<ApiDisallowedEmailAddressResponse> GetDisallowedEmailAddresses(RequestContext context, DataContext dataContext, GameUser user)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<DisallowedEmailAddress> Addresses = dataContext.Database.GetDisallowedEmailAddresses(skip, count);
        return DatabaseListExtensions.FromOldList<ApiDisallowedEmailAddressResponse, DisallowedEmailAddress>(Addresses, dataContext);
    }

    [ApiV3Endpoint("admin/disallowed/emailAddresses/address/{address}", HttpMethods.Post), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Adds an email address to the list of disallowed addresses.")]
    public ApiResponse<ApiDisallowedEmailAddressResponse> DisallowEmailAddress(RequestContext context, DataContext dataContext, GameUser user,
        string address, ApiModerationRequest body)
    {
        (DisallowedEmailAddress disallowed, bool success) = dataContext.Database.DisallowEmailAddress(address, body.Reason ?? "");
        return new(ApiDisallowedEmailAddressResponse.FromOld(disallowed, dataContext)!, success ? Created : OK);
    }

    [ApiV3Endpoint("admin/disallowed/emailAddresses/address/{address}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Removes an email address from the list of disallowed addresses.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotModifyUserDueToLowRoleErrorWhen)]
    public ApiOkResponse ReallowEmailAddress(RequestContext context, DataContext dataContext, GameUser user,
        string address)
    {
        dataContext.Database.ReallowEmailAddress(address);
        return new ApiOkResponse();
    }

    #endregion
    #region Email domains

    [ApiV3Endpoint("admin/disallowed/emailDomains"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets a paginated list of disallowed email domains.")]
    public ApiListResponse<ApiDisallowedEmailDomainResponse> GetDisallowedEmailDomains(RequestContext context, DataContext dataContext, GameUser user)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<DisallowedEmailDomain> Domains = dataContext.Database.GetDisallowedEmailDomains(skip, count);
        return DatabaseListExtensions.FromOldList<ApiDisallowedEmailDomainResponse, DisallowedEmailDomain>(Domains, dataContext);
    }

    [ApiV3Endpoint("admin/disallowed/emailDomains/domain/{domain}", HttpMethods.Post), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Adds an email domain to the list of domains.")]
    public ApiResponse<ApiDisallowedEmailDomainResponse> DisallowEmailDomain(RequestContext context, DataContext dataContext, GameUser user,
        [DocSummary(SharedParamDescriptions.DomainToDisallowParam)] string domain, ApiModerationRequest body)
    {
        (DisallowedEmailDomain disallowed, bool success) = dataContext.Database.DisallowEmailDomain(domain, body.Reason ?? "");
        return new(ApiDisallowedEmailDomainResponse.FromOld(disallowed, dataContext)!, success ? Created : OK);
    }

    [ApiV3Endpoint("admin/disallowed/emailDomains/domain/{domain}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Removes an email domain from the list of domains.")]
    public ApiOkResponse ReallowEmailDomain(RequestContext context, DataContext dataContext, GameUser user,
        [DocSummary(SharedParamDescriptions.DomainToDisallowParam)] string domain)
    {
        dataContext.Database.ReallowEmailDomain(domain);
        return new ApiOkResponse();
    }

    #endregion
}