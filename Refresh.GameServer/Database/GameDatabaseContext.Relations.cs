using System.Diagnostics.Contracts;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Relations
{
    #region Favouriting Levels
    [Pure]
    private bool IsLevelFavouritedByUser(GameLevel level, GameUser user) => this.FavouriteLevelRelations
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    [Pure]
    public DatabaseList<GameLevel> GetLevelsFavouritedByUser(GameUser user, int count, int skip, LevelFilterSettings levelFilterSettings, GameUser? accessor) 
        => new(this.FavouriteLevelRelations
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level)
        .FilterByLevelFilterSettings(accessor, levelFilterSettings)
        .FilterByGameVersion(levelFilterSettings.GameVersion), skip, count);
    
    public int GetTotalLevelsFavouritedByUser(GameUser user) 
        => this.FavouriteLevelRelations
            .Count(r => r.User == user);
    
    public bool FavouriteLevel(GameLevel level, GameUser user)
    {
        if (this.IsLevelFavouritedByUser(level, user)) return false;
        
        FavouriteLevelRelation relation = new()
        {
            Level = level,
            User = user,
        };
        this.Write(() => this.FavouriteLevelRelations.Add(relation));

        this.CreateLevelFavouriteEvent(user, level);

        return true;
    }
    
    public bool UnfavouriteLevel(GameLevel level, GameUser user)
    {
        FavouriteLevelRelation? relation = this.FavouriteLevelRelations
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this.Write(() => this.FavouriteLevelRelations.Remove(relation));

        return true;
    }
    #endregion

    #region Favouriting Users

    [Pure]
    private bool IsUserFavouritedByUser(GameUser userToFavourite, GameUser userFavouriting) => this.FavouriteUserRelations
        .FirstOrDefault(r => r.UserToFavourite == userToFavourite && r.UserFavouriting == userFavouriting) != null;

    [Pure]
    public bool AreUsersMutual(GameUser user1, GameUser user2) =>
        this.IsUserFavouritedByUser(user1, user2) &&
        this.IsUserFavouritedByUser(user2, user1);

    [Pure]
    public IEnumerable<GameUser> GetUsersMutuals(GameUser user)
    {
        return this.GetUsersFavouritedByUser(user, 1000, 0).AsEnumerable()
            .Where(u => this.IsUserFavouritedByUser(user, u));
    }
    
    [Pure]
    public IEnumerable<GameUser> GetUsersFavouritedByUser(GameUser user, int count, int skip) => this.FavouriteUserRelations
        .Where(r => r.UserFavouriting == user)
        .AsEnumerable()
        .Select(r => r.UserToFavourite)
        .Skip(skip)
        .Take(count);
    
    public int GetTotalUsersFavouritedByUser(GameUser user)
        => this.FavouriteUserRelations
            .Count(r => r.UserFavouriting == user);
    
    public int GetTotalUsersFavouritingUser(GameUser user)
        => this.FavouriteUserRelations
            .Count(r => r.UserToFavourite == user);

    public bool FavouriteUser(GameUser userToFavourite, GameUser userFavouriting)
    {
        if (this.IsUserFavouritedByUser(userToFavourite, userFavouriting)) return false;
        
        FavouriteUserRelation relation = new()
        {
            UserToFavourite = userToFavourite,
            UserFavouriting = userFavouriting,
        };
        
        this.Write(() => this.FavouriteUserRelations.Add(relation));

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
        FavouriteUserRelation? relation = this.FavouriteUserRelations
            .FirstOrDefault(r => r.UserToFavourite == userToFavourite && r.UserFavouriting == userFavouriting);

        if (relation == null) return false;

        this.Write(() => this.FavouriteUserRelations.Remove(relation));

        return true;
    }

    #endregion

    #region Queueing
    [Pure]
    private bool IsLevelQueuedByUser(GameLevel level, GameUser user) => this.QueueLevelRelations
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    [Pure]
    public DatabaseList<GameLevel> GetLevelsQueuedByUser(GameUser user, int count, int skip, LevelFilterSettings levelFilterSettings, GameUser? accessor)
        => new(this.QueueLevelRelations
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level)
        .FilterByLevelFilterSettings(accessor, levelFilterSettings)
        .FilterByGameVersion(levelFilterSettings.GameVersion), skip, count);
    
    [Pure]
    public int GetTotalLevelsQueuedByUser(GameUser user) 
        => this.QueueLevelRelations
            .Count(r => r.User == user);
    
    public bool QueueLevel(GameLevel level, GameUser user)
    {
        if (this.IsLevelQueuedByUser(level, user)) return false;
        
        QueueLevelRelation relation = new()
        {
            Level = level,
            User = user,
        };
        this.Write(() => this.QueueLevelRelations.Add(relation));

        return true;
    }

    public bool DequeueLevel(GameLevel level, GameUser user)
    {
        QueueLevelRelation? relation = this.QueueLevelRelations
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this.Write(() => this.QueueLevelRelations.Remove(relation));

        return true;
    }

    public void ClearQueue(GameUser user)
    {
        this.Write(() => this.QueueLevelRelations.RemoveRange(this.QueueLevelRelations.Where(r => r.User == user)));
    }

    #endregion

    #region Rating and Reviewing

    private RateLevelRelation? GetRateRelationByUser(GameLevel level, GameUser user)
        => this.RateLevelRelations.FirstOrDefault(r => r.User == user && r.Level == level);

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
        if (level.GameVersion != TokenGame.LittleBigPlanetPSP && !this.HasUserPlayedLevel(level, user)) return false;
        
        RateLevelRelation? rating = this.GetRateRelationByUser(level, user);
        
        if (rating == null)
        {
            rating = new RateLevelRelation
            {
                Level = level,
                User = user,
                RatingType = type,
                Timestamp = this._time.Now,
            };

            this.Write(() => this.RateLevelRelations.Add(rating));
            return true;
        }

        this.Write(() =>
        {
            rating.RatingType = type;
            rating.Timestamp = this._time.Now;
        });
        return true;
    }
    
    /// <summary>
    /// Adds a review to the database, deleting any old ones by the user on that level.
    /// </summary>
    /// <param name="review">The review to add</param>
    /// <param name="level">The level the review is for</param>
    /// <param name="user">The user who made the review</param>
    public void AddReviewToLevel(GameReview review, GameLevel level)
    {
        List<GameReview> toRemove = level.Reviews.Where(r => r.Publisher.UserId == review.Publisher.UserId).ToList();
        if (toRemove.Count > 0)
        {
            this.Write(() =>
            {
                foreach (GameReview reviewToDelete in toRemove)
                {
                    level.Reviews.Remove(reviewToDelete);
                    this.GameReviews.Remove(reviewToDelete);
                }
            });
        }
        
        this.AddSequentialObject(review, level.Reviews);
    }

    public DatabaseList<GameReview> GetReviewsByUser(GameUser user, int count, int skip)
    {
        return new DatabaseList<GameReview>(this.GameReviews
            .Where(r => r.Publisher == user), skip, count);
    }

    public int GetTotalReviewsByUser(GameUser user)
        => this.GameReviews.Count(r => r.Publisher == user);
    
    public void DeleteReview(GameReview review)
    {
        this.GameReviews.Remove(review);
    }
    
    public GameReview? GetReviewByLevelAndUser(GameLevel level, GameUser user)
    {
        return level.Reviews.FirstOrDefault(r => r.Publisher.UserId == user.UserId);
    }

    public DatabaseList<GameReview> GetReviewsForLevel(GameLevel level, int count, int skip)
    {
        return new DatabaseList<GameReview>(this.GameReviews
            .Where(r => r.Level == level), skip, count);
    }
    
    public int GetTotalReviewsForLevel(GameLevel level)
        => this.GameReviews.Count(r => r.Level == level);

    #endregion

    #region Playing
    
    public void PlayLevel(GameLevel level, GameUser user, int count)
    {
        PlayLevelRelation relation = new()
        {
            Level = level,
            User = user,
            Timestamp = this._time.TimestampMilliseconds,
            Count = count,
        };
        
        UniquePlayLevelRelation? uniqueRelation = this.UniquePlayLevelRelations
            .FirstOrDefault(r => r.Level == level && r.User == user);

        this.Write(() =>
        {
            this.PlayLevelRelations.Add(relation);
            
            // If the user hasn't played the level before, then add a unique relation too
            if (uniqueRelation == null)
            {
                this.UniquePlayLevelRelations.Add(new UniquePlayLevelRelation
                {
                    Level = level,
                    User = user,
                    Timestamp = this._time.TimestampMilliseconds,
                });

                this.CreateLevelPlayEvent(user, level);
            }
        });
    }

    public bool HasUserPlayedLevel(GameLevel level, GameUser user) =>
        this.UniquePlayLevelRelations
            .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    #endregion

    #region Comments

    private CommentRelation? GetCommentRelationByUser(GameComment comment, GameUser user) => this.CommentRelations
        .FirstOrDefault(r => r.Comment == comment && r.User == user);
    
    /// <summary>
    /// Get a user's rating on a particular comment.
    /// A null return value means a user has not set a rating.
    /// </summary>
    /// <param name="comment">The comment to check</param>
    /// <param name="user">The user to check</param>
    /// <returns>The rating if found</returns>
    [Pure]
    public RatingType? GetRatingByUser(GameComment comment, GameUser user) 
        => this.GetCommentRelationByUser(comment, user)?.RatingType;
    
    public int GetTotalRatingsForComment(GameComment comment, RatingType type) =>
        this.CommentRelations.Count(r => r.Comment == comment && r._RatingType == (int)type);
    
    public bool RateComment(GameUser user, GameComment comment, RatingType ratingType)
    {
        if (ratingType == RatingType.Neutral)
            return false;
        
        CommentRelation? relation = GetCommentRelationByUser(comment, user);

        if (relation == null)
        {
            relation = new CommentRelation
            {
                User = user,
                Comment = comment,
                Timestamp = this._time.Now,
                RatingType = ratingType,
            };
            
            this.Write(() =>
            {
                this.CommentRelations.Add(relation);
            });
        }
        else
        {
            this.Write(() =>
            {
                relation.Timestamp = this._time.Now;
                relation.RatingType = ratingType;
            });
        }

        return true;
    }

    #endregion
}