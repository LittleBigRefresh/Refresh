using System.Diagnostics.Contracts;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // Relations
{
    #region Favouriting Levels
    [Pure]
    private bool IsLevelFavouritedByUser(GameLevel level, GameUser user) => this.All<FavouriteLevelRelation>()
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    [Pure]
    public DatabaseList<GameLevel> GetLevelsFavouritedByUser(GameUser user, int count, int skip, LevelFilterSettings levelFilterSettings) 
        => new(this.All<FavouriteLevelRelation>()
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level)
        .FilterByLevelFilterSettings(null, levelFilterSettings)
        .FilterByGameVersion(levelFilterSettings.GameVersion), skip, count);
    
    public bool FavouriteLevel(GameLevel level, GameUser user)
    {
        if (this.IsLevelFavouritedByUser(level, user)) return false;
        
        FavouriteLevelRelation relation = new()
        {
            Level = level,
            User = user,
        };
        this.Write(() => this.Add(relation));

        this.CreateLevelFavouriteEvent(user, level);

        return true;
    }
    
    public bool UnfavouriteLevel(GameLevel level, GameUser user)
    {
        FavouriteLevelRelation? relation = this.All<FavouriteLevelRelation>()
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this.Write(() => this.Remove(relation));

        return true;
    }
    #endregion

    #region Favouriting Users

    [Pure]
    private bool IsUserFavouritedByUser(GameUser userToFavourite, GameUser userFavouriting) => this.All<FavouriteUserRelation>()
        .FirstOrDefault(r => r.UserToFavourite == userToFavourite && r.UserFavouriting == userFavouriting) != null;

    [Pure]
    public bool AreUsersMutual(GameUser user1, GameUser user2) =>
        this.IsUserFavouritedByUser(user1, user2) &&
        this.IsUserFavouritedByUser(user2, user1);
    
    [Pure]
    public IEnumerable<GameUser> GetUsersFavouritedByUser(GameUser user, int count, int skip) => this.All<FavouriteUserRelation>()
        .Where(r => r.UserFavouriting == user)
        .AsEnumerable()
        .Select(r => r.UserToFavourite)
        .Skip(skip)
        .Take(count);

    public bool FavouriteUser(GameUser userToFavourite, GameUser userFavouriting)
    {
        if (this.IsUserFavouritedByUser(userToFavourite, userFavouriting)) return false;
        
        FavouriteUserRelation relation = new()
        {
            UserToFavourite = userToFavourite,
            UserFavouriting = userFavouriting,
        };
        
        this.Write(() => this.Add(relation));

        this.CreateUserFavouriteEvent(userFavouriting, userToFavourite);

        if (this.AreUsersMutual(userFavouriting, userToFavourite))
        {
            this.AddNotification("New mutual", $"You are now mutuals with {userFavouriting.Username}!", userToFavourite);
            this.AddNotification("New mutual", $"You are now mutuals with {userToFavourite.Username}!", userFavouriting);
        }
        else
        {
            this.AddNotification("New follower", $"{userFavouriting.Username} hearted you!", userToFavourite);
        }

        return true;
    }

    public bool UnfavouriteUser(GameUser userToFavourite, GameUser userFavouriting)
    {
        FavouriteUserRelation? relation = this.All<FavouriteUserRelation>()
            .FirstOrDefault(r => r.UserToFavourite == userToFavourite && r.UserFavouriting == userFavouriting);

        if (relation == null) return false;

        this.Write(() => this.Remove(relation));

        return true;
    }

    #endregion

    #region Queueing
    [Pure]
    private bool IsLevelQueuedByUser(GameLevel level, GameUser user) => this.All<QueueLevelRelation>()
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    [Pure]
    public DatabaseList<GameLevel> GetLevelsQueuedByUser(GameUser user, int count, int skip, LevelFilterSettings levelFilterSettings)
        => new(this.All<QueueLevelRelation>()
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level)
        .FilterByLevelFilterSettings(null, levelFilterSettings)
        .FilterByGameVersion(levelFilterSettings.GameVersion), skip, count);
    
    public bool QueueLevel(GameLevel level, GameUser user)
    {
        if (this.IsLevelQueuedByUser(level, user)) return false;
        
        QueueLevelRelation relation = new()
        {
            Level = level,
            User = user,
        };
        this.Write(() => this.Add(relation));

        return true;
    }

    public bool DequeueLevel(GameLevel level, GameUser user)
    {
        QueueLevelRelation? relation = this.All<QueueLevelRelation>()
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this.Write(() => this.Remove(relation));

        return true;
    }

    public void ClearQueue(GameUser user)
    {
        this.Write(() => this.RemoveRange(user.QueueLevelRelations));
    }

    #endregion

    #region Rating and Reviewing

    private RateLevelRelation? GetRateRelationByUser(GameLevel level, GameUser user)
        => this.All<RateLevelRelation>().FirstOrDefault(r => r.User == user && r.Level == level);

    /// <summary>
    /// Get a user's rating on a particular level.
    /// A null return value means a user has not set a rating.
    /// On LBP1/PSP, a missing rating is a separate condition that should be sent
    /// while on LBP2 and newer you should return a Neutral rating.
    /// </summary>
    /// <param name="level">The level to check</param>
    /// <param name="user">The user to check</param>
    /// <returns>The rating if found</returns>
    [Pure]
    public RatingType? GetRatingByUser(GameLevel level, GameUser user) => this.GetRateRelationByUser(level, user)?.RatingType;

    public bool RateLevel(GameLevel level, GameUser user, RatingType type)
    {
        if (level.Publisher?.UserId == user.UserId) return false;
        if (!this.HasUserPlayedLevel(level, user)) return false;
        
        RateLevelRelation? rating = this.GetRateRelationByUser(level, user);
        
        if (rating == null)
        {
            rating = new RateLevelRelation
            {
                Level = level,
                User = user,
                RatingType = type,
                Timestamp = this.Time.Now,
            };

            this.Write(() => this.Add(rating));
            return true;
        }

        this.Write(() =>
        {
            rating.RatingType = type;
            rating.Timestamp = this.Time.Now;
        });
        return true;
    }

    #endregion

    #region Playing
    
    public void PlayLevel(GameLevel level, GameUser user, int count)
    {
        PlayLevelRelation relation = new()
        {
            Level = level,
            User = user,
            Timestamp = this.Time.TimestampMilliseconds,
            Count = count,
        };
        
        UniquePlayLevelRelation? uniqueRelation = this.All<UniquePlayLevelRelation>()
            .FirstOrDefault(r => r.Level == level && r.User == user);

        this.Write(() =>
        {
            this.Add(relation);
            
            // If the user hasn't played the level before, then add a unique relation too
            if (uniqueRelation == null) this.Add(new UniquePlayLevelRelation 
            {
                Level = level,
                User = user,
                Timestamp = this.Time.TimestampMilliseconds,
            });
        });

        this.CreateLevelPlayEvent(user, level);
    }

    public bool HasUserPlayedLevel(GameLevel level, GameUser user) =>
        this.All<UniquePlayLevelRelation>()
            .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    #endregion
}