using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using MongoDB.Bson;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminLevelApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/levels/id/{id}/teamPick", HttpMethods.Post), MinimumRole(GameUserRole.Curator)]
    [DocSummary("Marks a level as team picked.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse AddTeamPickToLevel(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GameLevel? level = database.GetLevelById(id); 
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        database.AddTeamPickToLevel(level);
        database.CreateLevelTeamPickEvent(user, level);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/levels/id/{id}/removeTeamPick", HttpMethods.Post), MinimumRole(GameUserRole.Curator)]
    [DocSummary("Removes a level's team pick status.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse RemoveTeamPickFromLevel(RequestContext context, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        database.RemoveTeamPickFromLevel(level);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/levels/id/{id}", HttpMethods.Patch), MinimumRole(GameUserRole.Curator)]
    [DocSummary("Updates a level.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForObjectWhen)]
    public ApiResponse<ApiGameLevelResponse> EditLevelById(RequestContext context, GameDatabaseContext database,
        GameUser user,
        [DocSummary("The ID of the level")] int id, ApiAdminEditLevelRequest body, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        if (body.IconHash != null && body.IconHash.StartsWith('g') &&
            !dataContext.GuidChecker.IsTextureGuid(level.GameVersion, long.Parse(body.IconHash)))
            return ApiValidationError.InvalidTextureGuidError;
        
        level = database.UpdateLevel(body, level, user);

        return ApiGameLevelResponse.FromOld(level, dataContext);
    }
    
    [ApiV3Endpoint("admin/levels/id/{id}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes a level.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse DeleteLevel(RequestContext context, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        database.DeleteLevel(level);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/levels/id/{id}/setAuthor", HttpMethods.Post), MinimumRole(GameUserRole.Curator)]
    [DocSummary("Changes the author of a level. The new author must be an existing user on the server..")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    public ApiOkResponse SetLevelAuthor(RequestContext context, GameDatabaseContext database, GameUser user, int id, ApiSetLevelAuthorRequest body)
    {
        if (!ObjectId.TryParse(body.AuthorId, out ObjectId authorId))
            return ApiValidationError.ObjectIdParseError;
        
        GameLevel? level = database.GetLevelById(id);
        if (level == null)
            return ApiNotFoundError.LevelMissingError;
        
        GameUser? newAuthor = database.GetUserByObjectId(authorId);
        if (newAuthor == null)
            return ApiNotFoundError.UserMissingError;

        database.UpdateLevelPublisher(level, newAuthor);
        return new ApiOkResponse();
    }
}