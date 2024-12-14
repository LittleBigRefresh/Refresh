using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
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

public class PlaylistLbp1Endpoints : EndpointGroup
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
        
        // TODO: when we get postgres, this can be IQueryable and we wont need ToList()
        IList<GamePlaylist> subPlaylists = dataContext.Database.GetPlaylistsInPlaylist(playlist).ToList();
        // TODO: when we get postgres, this can be IQueryable and we wont need ToList()
        IList<GameLevel> levels = dataContext.Database.GetLevelsInPlaylist(playlist, dataContext.Game).ToList();                    
                    
        (int skip, int count) = context.GetPageData();
        
        int total = subPlaylists.Count + levels.Count;

        // Concat together the playlist's sub-playlists and levels 
        IEnumerable<GameMinimalLevelResponse> slots =
            GameMinimalLevelResponse.FromOldList(subPlaylists, dataContext) // the sub-playlists
                .Concat(GameMinimalLevelResponse.FromOldList(levels, dataContext)) // the sub-levels
                .Skip(skip).Take(count);
        
        // Convert the GameLevelResponse list down to a GameMinimalLevelResponse
        return new SerializedMinimalLevelList(
            slots,
            total,
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

        // Get the playlists which contain the level/playlist, and if we have an author specified, filter it down to only playlists which are created by the author
        // TODO: with postgres this can be IQueryable, and we dont need List
        List<GamePlaylist> playlists;
        if (slotType == "playlist")
        {
            GamePlaylist? playlist = dataContext.Database.GetPlaylistById(slotId);
            if (playlist == null)
                return null;

            playlists = author == null ? 
                dataContext.Database.GetPlaylistsContainingPlaylist(playlist).ToList() : 
                dataContext.Database.GetPlaylistsByAuthorContainingPlaylist(author, playlist).ToList();
        }
        else
        {
            GameLevel? level = dataContext.Database.GetLevelByIdAndType(slotType, slotId);
            if (level == null)
                return null;
            
            playlists = author == null ? 
                dataContext.Database.GetPlaylistsContainingLevel(level).ToList() : 
                dataContext.Database.GetPlaylistsByAuthorContainingLevel(author, level).ToList();
        }
        
        int total = playlists.Count;
        
        (int skip, int count) = context.GetPageData();

        // Return the serialized playlists 
        return new SerializedMinimalLevelList(
            GameMinimalLevelResponse.FromOldList(playlists.Skip(skip).Take(count), dataContext), 
            total, 
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

            // If the parent contains the child in its parent tree, block the request to prevent recursive playlists
            // This would be a `BadRequest`, but the game has a bug and will do this when creating sub-playlists,
            // so lets not upset it and just return OK, I dont expect this to be a common problem for people to run into.
            bool recursive = false;
            parentPlaylist.TraverseParentsRecursively(database, delegate(GamePlaylist playlist)
            {
                if (playlist.PlaylistId == childPlaylist.PlaylistId)
                    recursive = true;   
            });
            if (recursive) return OK;
            
            // Add the playlist to the parent   
            database.AddPlaylistToPlaylist(childPlaylist, parentPlaylist);

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