using Refresh.GameServer.Authentication;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Playlists
{
    public GamePlaylist CreatePlaylist(GameUser user, SerializedLbp1Playlist createInfo, bool rootPlaylist)
    {
        GamePlaylist playlist = GamePlaylist.ToGamePlaylist(createInfo, user, rootPlaylist);
        this.CreatePlaylist(playlist);
        return playlist;
    }

    public GamePlaylist CreatePlaylist(GameUser user, SerializedLbp3Playlist createInfo, bool rootPlaylist)
    {
        GamePlaylist playlist = GamePlaylist.ToGamePlaylist(createInfo, user, rootPlaylist);
        this.CreatePlaylist(playlist);
        return playlist;
    }

    public void CreatePlaylist(GamePlaylist createInfo)
    {
        this.Write(() =>
        {
            this.AddSequentialObject(createInfo);
            createInfo.CreationDate = this._time.Now;
            createInfo.LastUpdateDate = this._time.Now;
        });
    }

    public GamePlaylist? GetPlaylistById(int playlistId) 
        => this.GamePlaylists.FirstOrDefault(p => p.PlaylistId == playlistId);

    public void UpdatePlaylist(GamePlaylist playlist, SerializedLbp1Playlist updateInfo)
    {
        this.Write(() =>
        {
            playlist.Name = updateInfo.Name;
            playlist.Description = updateInfo.Description;
            playlist.IconHash = updateInfo.Icon;
            playlist.LocationX = updateInfo.Location.X;
            playlist.LocationY = updateInfo.Location.Y;
            playlist.LastUpdateDate = this._time.Now;
        });
    }

    public void UpdatePlaylist(GamePlaylist playlist, SerializedLbp3Playlist updateInfo)
    {
        this.Write(() =>
        {
            if (updateInfo.Name != null) playlist.Name = updateInfo.Name;
            if (updateInfo.Description != null) playlist.Description = updateInfo.Description;
            playlist.LastUpdateDate = this._time.Now;
        });
    }

    public void DeletePlaylist(GamePlaylist playlist)
    {
        this.Write(() =>
        {
            // Remove all relations relating to this playlist
            this.LevelPlaylistRelations.RemoveRange(l => l.Playlist == playlist);
            this.SubPlaylistRelations.RemoveRange(l => l.Playlist == playlist || l.SubPlaylist == playlist);
            this.FavouritePlaylistRelations.RemoveRange(l => l.Playlist == playlist);
            
            // Remove the playlist object
            this.GamePlaylists.Remove(playlist);
        });
    }

    public void AddPlaylistToPlaylist(GamePlaylist child, GamePlaylist parent)
    {
        this.Write(() =>
        {
            // Make sure to not create a duplicate object
            if (this.SubPlaylistRelations.Any(p => p.SubPlaylist == child && p.Playlist == parent))
                return;
            
            // Add the relation
            this.SubPlaylistRelations.Add(new SubPlaylistRelation
            {
                Playlist = parent,
                SubPlaylist = child,
            });
        });
    }
    
    public void RemovePlaylistFromPlaylist(GamePlaylist child, GamePlaylist parent)
    {
        this.Write(() =>
        {
            SubPlaylistRelation? relation =
                this.SubPlaylistRelations.FirstOrDefault(r => r.SubPlaylist == child && r.Playlist == parent);

            if (relation == null)
                return;
            
            this.SubPlaylistRelations.Remove(relation);
        });
    }
    
    public void AddLevelToPlaylist(GameLevel level, GamePlaylist parent)
    {
        this.Write(() =>
        {
            // Make sure to not create a duplicate object
            if (this.LevelPlaylistRelations.Any(p => p.Level == level && p.Playlist == parent))
                return;
            
            // Add the relation
            this.LevelPlaylistRelations.Add(new LevelPlaylistRelation
            {
                Level = level,
                Playlist = parent,
                // Currently only LBP3 cares about custom playlist level order anyway
                Index = this.GetLevelsInPlaylist(parent, TokenGame.LittleBigPlanet3).Count(),
            });
        });
    }
    
    public void RemoveLevelFromPlaylist(GameLevel level, GamePlaylist parent)
    {
        LevelPlaylistRelation? relation =
            this.LevelPlaylistRelations.FirstOrDefault(r => r.Level == level && r.Playlist == parent);

        if (relation == null)
            return;

        // decrease index of every playlist level after this one by 1
        this.DecreasePlaylistLevelIndicesAfterIndex(parent, relation.Index);
        
        this.Write(() =>
        {
            this.LevelPlaylistRelations.Remove(relation);
        });
    }

    private void DecreasePlaylistLevelIndicesAfterIndex(GamePlaylist playlist, int index)
    {
        IEnumerable<LevelPlaylistRelation> relations = this.LevelPlaylistRelations
            .Where(r => r.Playlist == playlist && r.Index >= index)
            .AsEnumerable();

        
        foreach(LevelPlaylistRelation relation in relations)
        {
            this.Write(() => {
                relation.Index--;
            });
        }
    }

    public void SetPlaylistLevelIndex(GamePlaylist playlist, GameLevel level, int newIndex)
    {
        LevelPlaylistRelation relation = this.LevelPlaylistRelations
            .First(r => r.Playlist == playlist && r.Level == level);

        this.Write(() => {
            relation.Index = newIndex;
        });
    }

    public IEnumerable<GamePlaylist> GetPlaylistsContainingPlaylist(GamePlaylist playlist)
        // TODO: with postgres this can be IQueryable
        => this.SubPlaylistRelations.Where(p => p.SubPlaylist == playlist).AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId))
            .Where(p => !p.IsRoot);

    public IEnumerable<GamePlaylist> GetPlaylistsByAuthorContainingPlaylist(GameUser user, GamePlaylist playlist)
        // TODO: with postgres this can be IQueryable
        => this.SubPlaylistRelations.Where(p => p.SubPlaylist == playlist).AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId))
            .Where(p => p.Publisher.UserId == user.UserId)
            .Where(p => !p.IsRoot);

    public IEnumerable<GameLevel> GetLevelsInPlaylist(GamePlaylist playlist, TokenGame game)
    {
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        IEnumerable<LevelPlaylistRelation> relations = this.LevelPlaylistRelations
            .Where(l => l.Playlist == playlist).AsEnumerable();

        // Only sort by order if needed, to improve performance
        if (game == TokenGame.LittleBigPlanet3)
            relations = relations.OrderBy(r => r.Index);

        return relations.Select(l => l.Level).FilterByGameVersion(game);
    }

    public int GetTotalLevelsInPlaylistCount(GamePlaylist playlist, TokenGame game) => 
        this.LevelPlaylistRelations.Where(l => l.Playlist == playlist).AsEnumerable()
            .Select(l => l.Level)
            .FilterByGameVersion(game)
            .Count();

    public IEnumerable<GamePlaylist> GetPlaylistsInPlaylist(GamePlaylist playlist)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.SubPlaylistRelations.Where(p => p.Playlist == playlist).AsEnumerable()
            .Select(l => l.SubPlaylist);

    public IEnumerable<GamePlaylist> GetPlaylistsByAuthor(GameUser author)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.GamePlaylists.Where(p => p.Publisher == author).AsEnumerable()
            .Where(p => !p.IsRoot);

    public IEnumerable<GamePlaylist> GetPlaylistsByAuthorContainingLevel(GameUser author, GameLevel level)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.LevelPlaylistRelations.Where(p => p.Level == level).AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId))
            .Where(p => p.Publisher.UserId == author.UserId);
    
    public IEnumerable<GamePlaylist> GetPlaylistsContainingLevel(GameLevel level)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.LevelPlaylistRelations.Where(p => p.Level == level).AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId));

    public bool IsPlaylistFavouritedByUser(GamePlaylist playlist, GameUser user)
        => this.FavouritePlaylistRelations.FirstOrDefault(r => r.Playlist == playlist && r.User == user) != null;

    public IEnumerable<GamePlaylist> GetPlaylistsFavouritedByUser(GameUser user) => this.FavouritePlaylistRelations
        .Where(r => r.User == user).AsEnumerable()
        .Select(r => r.Playlist);

    public int GetFavouriteCountForPlaylist(GamePlaylist playlist)
        => this.FavouritePlaylistRelations
            .Count(r => r.Playlist == playlist);

    public bool FavouritePlaylist(GamePlaylist playlist, GameUser user)
    {
        if (this.IsPlaylistFavouritedByUser(playlist, user)) return false;

        FavouritePlaylistRelation relation = new()
        {
            Playlist = playlist,
            User = user, 
        };
        this.Write(() => this.FavouritePlaylistRelations.Add(relation));

        return true;
    }

    public bool UnfavouritePlaylist(GamePlaylist playlist, GameUser user)
    {
        FavouritePlaylistRelation? relation = this.FavouritePlaylistRelations
            .FirstOrDefault(r => r.Playlist == playlist && r.User == user);

        if (relation == null) return false;

        this.Write(() => this.FavouritePlaylistRelations.Remove(relation));

        return true;
    }
}