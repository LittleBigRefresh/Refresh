using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Playlists;

public class Lbp1PlaylistEndpoints : EndpointGroup
{
    // Creates a playlist, with an optional parent ID
    [GameEndpoint("createPlaylist", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response CreatePlaylist(RequestContext context, DataContext dataContext, SerializedLbp1Playlist body)
    {
        GameUser user = dataContext.User!;
        
        GamePlaylist? parent = null;
        // If the parent ID is specified, try to parse that out
        if (int.TryParse(context.QueryString["parent_id"], out int parentId))
        {
            parent = dataContext.Database.GetPlaylistById(parentId);

            // Dont try to parent to a non-existent parent playlist
            if (parent == null)
                return BadRequest;
            
            // Dont let you create a sub-playlist of someone else's playlist
            if (user.UserId != parent.Publisher.UserId)
                return Unauthorized;

            // If the user has no root playlist, but they are trying to create a sub-playlist, something has gone wrong.
            if (user.RootPlaylist == null)
                return BadRequest;
        }

        // Create the playlist, marking it as the root playlist if the user does not have one set already
        GamePlaylist playlist = dataContext.Database.CreatePlaylist(user, body, user.RootPlaylist == null);

        // If there is a parent, add the new playlist to the parent
        if (parent != null) 
            dataContext.Database.AddPlaylistToPlaylist(playlist, parent);

        // If this new playlist is the root playlist, mark the user's root playlist as it
        if (playlist.IsRoot)
            dataContext.Database.SetUserRootPlaylist(user, playlist);
        
        // Create the new playlist, returning the data
        return new Response(SerializedLbp1Playlist.FromOld(playlist, dataContext), ContentType.Xml);
    }

    // Gets the slots contained within a playlist
    [GameEndpoint("playlist/{id}", HttpMethods.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList? GetPlaylistSlots(RequestContext context, DataContext dataContext, int id)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null)
            return null;

        (int skip, int count) = context.GetPageData();
        
        DatabaseList<GamePlaylist> subPlaylists = dataContext.Database.GetPlaylistsInPlaylist(playlist, skip, count);
        DatabaseList<GameLevel> levels = dataContext.Database.GetLevelsInPlaylist(playlist, dataContext.Game, skip, count);                    

        // Concat together the playlist's sub-playlists and levels 
        IEnumerable<SerializedMinimalLevelResponse> slots =
            SerializedMinimalLevelResponse.FromOldList(subPlaylists.Items, dataContext) // the sub-playlists
                .Concat(SerializedMinimalLevelResponse.FromOldList(levels.Items, dataContext)); // the sub-levels
        
        // Convert the GameLevelResponse list down to a SerializedMinimalLevelResponse
        return new SerializedMinimalLevelList(
            slots,
            subPlaylists.TotalItems + levels.TotalItems,
            skip
        );
    }

    [GameEndpoint("playlistsContainingSlotByAuthor/{slotType}/{slotId}", ContentType.Xml)]
    [GameEndpoint("playlistsContainingSlot/{slotType}/{slotId}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedMinimalLevelList? PlaylistsContainingSlot(RequestContext context, DataContext dataContext,
        string slotType, int slotId)
    {
        string? authorName = context.QueryString["author"];

        GameUser? author = null; 
        // Allow the author to be unspecified
        if (authorName != null)
        {
            author = dataContext.Database.GetUserByUsername(authorName);
            if (author == null)
                return null;
        }

        (int skip, int count) = context.GetPageData();

        // Get the playlists which contain the level/playlist, and if we have an author specified, filter it down to only playlists which are created by the author
        // TODO: with postgres this can be IQueryable, and we dont need List
        DatabaseList<GamePlaylist> playlists;
        if (slotType == "playlist")
        {
            GamePlaylist? playlist = dataContext.Database.GetPlaylistById(slotId);
            if (playlist == null)
                return null;

            playlists = author == null ? 
                dataContext.Database.GetPlaylistsContainingPlaylist(playlist, skip, count) : 
                dataContext.Database.GetPlaylistsByAuthorContainingPlaylist(author, playlist, skip, count);
        }
        else
        {
            GameLevel? level = dataContext.Database.GetLevelByIdAndType(slotType, slotId);
            if (level == null)
                return null;
            
            playlists = author == null ? 
                dataContext.Database.GetPlaylistsContainingLevel(level, skip, count) : 
                dataContext.Database.GetPlaylistsByAuthorContainingLevel(author, level, skip, count);
        }

        // Return the serialized playlists 
        return new SerializedMinimalLevelList(
            SerializedMinimalLevelResponse.FromOldList(playlists.Items, dataContext), 
            playlists.TotalItems, 
            skip
        );
    }

    [GameEndpoint("setPlaylistMetaData/{id}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UpdatePlaylistMetadata(RequestContext context, GameDatabaseContext database, GameUser user, int id, SerializedLbp1Playlist body)
    {
        GamePlaylist? playlist = database.GetPlaylistById(id);
        if (playlist == null)
            return NotFound;

        // Dont allow the wrong user to update playlists
        if (playlist.Publisher.UserId != user.UserId)
            return Unauthorized;
        
        database.UpdatePlaylist(playlist, body);
        return OK;
    }

    [GameEndpoint("deletePlaylist/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response DeletePlaylist(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GamePlaylist? playlist = database.GetPlaylistById(id);
        if (playlist == null)
            return NotFound;

        // Dont allow the wrong user to delete playlists
        if (playlist.Publisher.UserId != user.UserId)
            return Unauthorized;

        database.DeletePlaylist(playlist);
        return OK;
    }

    [GameEndpoint("addToPlaylist/{playlistId}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response AddSlotToPlaylist(RequestContext context, GameDatabaseContext database, GameUser user, int playlistId)
    {
        string? slotType = context.QueryString["slot_type"];
        if (slotType == null)
            return BadRequest;
        
        if (!int.TryParse(context.QueryString["slot_id"], out int slotId))
            return BadRequest;

        GamePlaylist? parentPlaylist = database.GetPlaylistById(playlistId);
        if (parentPlaylist == null)
            return NotFound;

        // Dont let people add slots to other's playlists
        if (parentPlaylist.Publisher.UserId != user.UserId)
            return Unauthorized;

        // Adding a playlist to a playlist requires a special case, since we use `SubPlaylistRelation` internally to record child playlists.
        if (slotType == "playlist")
        {
            GamePlaylist? childPlaylist = database.GetPlaylistById(slotId);

            // If the child doesn't exist, exit gracefully
            if (childPlaylist == null)
                return NotFound;

            // Dont allow a playlist to be a child of itself
            if (childPlaylist.PlaylistId == parentPlaylist.PlaylistId)
                return BadRequest;

            // Add the child playlist to the parent before checking for recursive playlists below to catch cases where a loop would
            // only happen once the child playlist is added to its new parent
            database.AddPlaylistToPlaylist(childPlaylist, parentPlaylist);

            // If the parent contains the child in its parent tree, block the request to prevent recursive playlists
            bool recursive = false;
            IEnumerable<GamePlaylist> traversedPlaylists = [childPlaylist];
            parentPlaylist.TraverseParentsRecursively(database, delegate(GamePlaylist playlist)
            {
                // If we have already traversed this playlist before, we have found a loop. Stop traversing in that case.
                if (traversedPlaylists.Contains(playlist))
                {
                    recursive = true;
                    return false;
                }
                else
                {
                    // Remember this playlist for loop detection
                    traversedPlaylists = traversedPlaylists.Append(playlist);
                    return true;
                }
            });
            if (recursive) {
                // If adding this playlist to its parent has caused a loop which was not there before, remove it from its parent
                database.RemovePlaylistFromPlaylist(childPlaylist, parentPlaylist);
                return BadRequest;
            }
            
            // ReSharper disable once ExtractCommonBranchingCode see like 3 lines below (line count subject to change)
            return OK;
        }
        // ReSharper disable once RedundantIfElseBlock i am intentionally writing this code like this to prevent code
        //                                             accidentally falling outside of the branch during a possible future
        //                                             refactor and returning OK when not intended. ok, rider? 
        else
        {
            GameLevel? level = database.GetLevelByIdAndType(slotType, slotId);
            if (level == null)
                return NotFound;
 
            database.AddLevelToPlaylist(level, parentPlaylist);
            return OK;
        }
    }

    [GameEndpoint("removeFromPlaylist/{playlistId}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response RemoveSlotFromPlaylist(RequestContext context, GameDatabaseContext database, GameUser user,
        int playlistId)
    {
        string? slotType = context.QueryString["slot_type"];
        if (slotType == null)
            return BadRequest;
        
        if (!int.TryParse(context.QueryString["slot_id"], out int slotId))
            return BadRequest;

        GamePlaylist? parentPlaylist = database.GetPlaylistById(playlistId);
        if (parentPlaylist == null)
            return NotFound;

        // Dont let people remove slots from other's playlists
        if (parentPlaylist.Publisher.UserId != user.UserId)
            return Unauthorized;

        // Removing a playlist from a playlist requires a special case, since we use `SubPlaylistRelation` internally to record child playlists.
        if (slotType == "playlist")
        {
            GamePlaylist? childPlaylist = database.GetPlaylistById(slotId);
            if (childPlaylist == null)
                return NotFound;
            

            database.RemovePlaylistFromPlaylist(childPlaylist, parentPlaylist);

            // ReSharper disable once ExtractCommonBranchingCode see like 3 lines below (line count subject to change)
            return OK;
        }
        // ReSharper disable once RedundantIfElseBlock i am intentionally writing this code like this to prevent code
        //                                             accidentally falling outside of the branch during a possible future
        //                                             refactor and returning OK when not intended. ok, rider? 
        else
        {
            GameLevel? level = database.GetLevelByIdAndType(slotType, slotId);
            if (level == null)
                return NotFound;

            database.RemoveLevelFromPlaylist(level, parentPlaylist);
            
            return OK;
        }
    }

    [GameEndpoint("moveFromPlaylist/{from}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response MoveSlotFromPlaylist(RequestContext context, GameDatabaseContext database, GameUser user, int from)     
    {
        if (!int.TryParse(context.QueryString["to"], out int to))
            return BadRequest;

        Response ret;
        
        if ((ret = this.RemoveSlotFromPlaylist(context, database, user, from)).StatusCode != OK)
            return ret;
        
        if ((ret = this.AddSlotToPlaylist(context, database, user, to)).StatusCode != OK)
            return ret;

        return OK;
    }
}