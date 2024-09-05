using Refresh.GameServer.Authentication;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Playlists
{
    public GamePlaylist CreatePlaylist(GameUser user, SerializedPlaylist createInfo, bool rootPlaylist)
    {
        GamePlaylist playlist = new()
        {
            Publisher = user, 
            Name = createInfo.Name,
            Description = createInfo.Description, 
            IconHash = createInfo.Icon, 
            LocationX = createInfo.Location.X, 
            LocationY = createInfo.Location.Y,
            IsRoot = rootPlaylist,
        };
        
        this.Write(() =>
        {
            this.AddSequentialObject(playlist);
        });
        
        return playlist;
    }

    public GamePlaylist? GetPlaylistById(int playlistId) 
        => this.GamePlaylists.FirstOrDefault(p => p.PlaylistId == playlistId);

    public void UpdatePlaylist(GamePlaylist playlist, SerializedPlaylist updateInfo)
    {
        this.Write(() =>
        {
            playlist.Name = updateInfo.Name;
            playlist.Description = updateInfo.Description;
            playlist.IconHash = updateInfo.Icon;
            playlist.LocationX = updateInfo.Location.X;
            playlist.LocationY = updateInfo.Location.Y;
        });
    }

    public void DeletePlaylist(GamePlaylist playlist)
    {
        this.Write(() =>
        {
            // Remove all relations relating to this playlist
            this.LevelPlaylistRelations.RemoveRange(l => l.Playlist == playlist);
            this.SubPlaylistRelations.RemoveRange(l => l.Playlist == playlist || l.SubPlaylist == playlist);
            
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
            });
        });
    }
    
    public void RemoveLevelFromPlaylist(GameLevel level, GamePlaylist parent)
    {
        this.Write(() =>
        {
            LevelPlaylistRelation? relation =
                this.LevelPlaylistRelations.FirstOrDefault(r => r.Level == level && r.Playlist == parent);

            if (relation == null)
                return;
            
            this.LevelPlaylistRelations.Remove(relation);
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

    public IEnumerable<GameLevel> GetLevelsInPlaylist(GamePlaylist playlist, TokenGame game) =>
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        this.LevelPlaylistRelations.Where(l => l.Playlist == playlist).AsEnumerable()
            .Select(l => l.Level)
            .FilterByGameVersion(game);

    public IEnumerable<GamePlaylist> GetPlaylistsInPlaylist(GamePlaylist playlist)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.SubPlaylistRelations.Where(p => p.Playlist == playlist).AsEnumerable()
            .Select(l => l.SubPlaylist);

    public IEnumerable<GamePlaylist> GetPlaylistsByAuthorContainingLevel(GameUser author, GameLevel level)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.LevelPlaylistRelations.Where(p => p.Level == level).AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId))
            .Where(p => p.Publisher.UserId == author.UserId);
    
    public IEnumerable<GamePlaylist> GetPlaylistsContainingLevel(GameLevel level)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.LevelPlaylistRelations.Where(p => p.Level == level).AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId));
}