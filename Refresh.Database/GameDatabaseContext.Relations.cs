using System.Diagnostics;
using System.Diagnostics.Contracts;
using Refresh.Database.Models;
using Refresh.Database.Query;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Relations;

namespace Refresh.Database;

public partial class GameDatabaseContext // Relations
{
    #region Favouriting Levels
    private IQueryable<FavouriteLevelRelation> FavouriteLevelRelationsIncluded => this.FavouriteLevelRelations
        .Include(r => r.Level)
        .Include(r => r.Level.Statistics)
        .Include(r => r.Level.Publisher);
    
    [Pure]
    public bool IsLevelFavouritedByUser(GameLevel level, GameUser user) => this.FavouriteLevelRelations
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    [Pure]
    public DatabaseList<GameLevel> GetLevelsFavouritedByUser(GameUser user, int count, int skip, LevelFilterSettings levelFilterSettings, GameUser? accessor) 
        => new(this.FavouriteLevelRelationsIncluded
        .Where(r => r.User == user)
        .OrderByDescending(r => r.Timestamp)
        .AsEnumerableIfRealm()
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
            Timestamp = this._time.Now,
        };
        
        this.WriteEnsuringStatistics(user, level, () =>
        {
            this.FavouriteLevelRelations.Add(relation);
            level.Statistics!.FavouriteCount++;
            user.Statistics!.FavouriteLevelCount++;

            if (level.Publisher == user)
                level.Statistics!.FavouriteCountExcludingPublisher++;
        });

        this.CreateLevelFavouriteEvent(user, level);

        return true;
    }
    
    public bool UnfavouriteLevel(GameLevel level, GameUser user)
    {
        FavouriteLevelRelation? relation = this.FavouriteLevelRelations
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this.WriteEnsuringStatistics(user, level, () =>
        {
            this.FavouriteLevelRelations.Remove(relation);
            level.Statistics!.FavouriteCount--;
            user.Statistics!.FavouriteLevelCount--;

            if (level.Publisher == user)
                level.Statistics!.FavouriteCountExcludingPublisher--;
        });

        return true;
    }
    
    public int GetTotalFavouritesForLevel(GameLevel level, bool includingAuthor = true) => this.FavouriteLevelRelations
            .Count(r => r.Level == level && (includingAuthor || r.User != level.Publisher));

    #endregion

    #region Favouriting Users

    private IQueryable<FavouriteUserRelation> FavouriteUserRelationsIncluded => this.FavouriteUserRelations
        .Include(r => r.UserFavouriting)
        .Include(r => r.UserToFavourite);

    [Pure]
    private bool IsUserFavouritedByUser(GameUser userToFavourite, GameUser userFavouriting) => this.FavouriteUserRelations
        .Any(r => r.UserToFavourite == userToFavourite && r.UserFavouriting == userFavouriting);

    [Pure]
    public bool AreUsersMutual(GameUser user1, GameUser user2) =>
        this.IsUserFavouritedByUser(user1, user2) &&
        this.IsUserFavouritedByUser(user2, user1);

    [Pure]
    public IEnumerable<GameUser> GetUsersMutuals(GameUser user)
    {
        return this.GetUsersFavouritedByUser(user, 1000, 0)
            .ToArray()
            .Where(u => this.IsUserFavouritedByUser(user, u));
    }
    
    [Pure]
    public IEnumerable<GameUser> GetUsersFavouritedByUser(GameUser user, int count, int skip) => this.FavouriteUserRelationsIncluded
        .Where(r => r.UserFavouriting == user)
        .OrderByDescending(r => r.Timestamp)
        .AsEnumerableIfRealm()
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
            Timestamp = this._time.Now,
        };
        
        this.WriteEnsuringStatistics(userFavouriting, () =>
        {
            userFavouriting.Statistics!.FavouriteUserCount++;
            this.FavouriteUserRelations.Add(relation);
        });
        
        this.WriteEnsuringStatistics(userToFavourite, () =>
        {
            userToFavourite.Statistics!.FavouriteCount++;
        });

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

        this.WriteEnsuringStatistics(userFavouriting, () =>
        {
            userFavouriting.Statistics!.FavouriteUserCount--;
            this.FavouriteUserRelations.Remove(relation);
        });
        
        this.WriteEnsuringStatistics(userToFavourite, () =>
        {
            userToFavourite.Statistics!.FavouriteCount--;
        });

        return true;
    }

    #endregion

    #region Queueing

    private IQueryable<QueueLevelRelation> QueueLevelRelationsIncluded => this.QueueLevelRelations
        .Include(r => r.Level)
        .Include(r => r.Level.Statistics)
        .Include(r => r.Level.Publisher);
    
    [Pure]
    public bool IsLevelQueuedByUser(GameLevel level, GameUser user) => this.QueueLevelRelations
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    [Pure]
    public DatabaseList<GameLevel> GetLevelsQueuedByUser(GameUser user, int count, int skip, LevelFilterSettings levelFilterSettings, GameUser? accessor)
        => new(this.QueueLevelRelationsIncluded
        .Where(r => r.User == user)
        .OrderByDescending(r => r.Timestamp)
        .AsEnumerableIfRealm()
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
            Timestamp = this._time.Now,
        };
        this.WriteEnsuringStatistics(user, () =>
        {
            user.Statistics!.QueueCount++;
            this.QueueLevelRelations.Add(relation);
        });

        return true;
    }

    public bool DequeueLevel(GameLevel level, GameUser user)
    {
        QueueLevelRelation? relation = this.QueueLevelRelations
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this.WriteEnsuringStatistics(user, () =>
        {
            user.Statistics!.QueueCount--;
            this.QueueLevelRelations.Remove(relation);
        });

        return true;
    }

    public void ClearQueue(GameUser user)
    {
        this.WriteEnsuringStatistics(user, () =>
        {
            user.Statistics!.QueueCount = 0;
            this.QueueLevelRelations.RemoveRange(r => r.User == user);
        });
    }

    #endregion

    #region Rating and Reviewing

    public IQueryable<RateReviewRelation> RateReviewRelationsIncluded => this.RateReviewRelations
        .Include(r => r.Review)
        .Include(r => r.Review.Level)
        .Include(r => r.Review.Level.Statistics)
        .Include(r => r.Review.Level.Publisher)
        .Include(r => r.Review.Publisher)
        .Include(r => r.User);

    public IQueryable<RateLevelRelation> RateLevelRelationsIncluded => this.RateLevelRelations
        .Include(r => r.User)
        .Include(r => r.Level)
        .Include(r => r.Level.Statistics)
        .Include(r => r.Level.Publisher);

    public IQueryable<GameReview> GameReviewsIncluded => this.GameReviews
        .Include(r => r.Level)
        .Include(r => r.Level.Statistics)
        .Include(r => r.Level.Publisher)
        .Include(r => r.Publisher);

    public void RateReview(GameReview review, RatingType ratingType, GameUser user)
    {
        // If the rating type is neutral, remove the previous review rating by user 
        if (ratingType == RatingType.Neutral && this.ReviewRatingExistsByUser(user, review))
        {
            RateReviewRelation relation =
                this.RateReviewRelations.First(r => r.Review == review && r.User == user);
            this.Write(() => this.RateReviewRelations.Remove(relation));

            return;
        }
        
        // If the relation already exists, set the new rating
        if (this.ReviewRatingExistsByUser(user, review))
        {
            RateReviewRelation relation = this.RateReviewRelations.First(r => r.Review == review && r.User == user);
            
            this.Write(() => {
                relation.RatingType = ratingType;
                relation.Timestamp = this._time.Now;
            });

            return;
        }
        
        RateReviewRelation reviewRelation = new()
        {
            RatingType = ratingType,
            Review = review,
            User = user,
            Timestamp = this._time.Now,
        };

        this.Write(() =>
        {
            this.RateReviewRelations.Add(reviewRelation);
        });
    }
    
    public DatabaseRating GetRatingForReview(GameReview review)
    {
        IEnumerable<RateReviewRelation> relations = this.RateReviewRelations.Where(r => r.Review == review);
        DatabaseRating rating = new();

        foreach (RateReviewRelation relation in relations)
        {
            if (relation.RatingType == RatingType.Yay)
                rating.PositiveRating++;
            else
                rating.NegativeRating++;
        }

        return rating;
    }

    public int GetRawRatingForReview(GameReview review)
    {
        IQueryable<RateReviewRelation> relations = this.RateReviewRelations.Where(r => r.Review == review);
        int rawRating = 0;

        foreach (RateReviewRelation relation in relations)
        {
            if (relation.RatingType == RatingType.Yay)
                rawRating++;
            else
                rawRating--;
        }

        return rawRating;
    }

    public GameReview? GetReviewByUserForLevel(GameUser user, GameLevel level)
        => this.GameReviewsIncluded.FirstOrDefault(gameReview => gameReview.Publisher == user && gameReview.Level == level);

    public bool ReviewRatingExistsByUser(GameUser user, GameReview review)
        => this.RateReviewRelations.Any(relation => relation.Review == review && relation.User == user);

    public RateReviewRelation? GetRateReviewRelationForReview(GameUser user, GameReview review)
        => this.RateReviewRelationsIncluded.FirstOrDefault(relation => relation.User == user && relation.Review == review);
    
    public bool ReviewRatingExists(GameUser user, GameReview review, RatingType rating)
        => this.RateReviewRelations.Any(r => r.Review == review && r.User == user && r.RatingType == rating);

    private RateLevelRelation? GetRateRelationByUser(GameLevel level, GameUser user)
        => this.RateLevelRelationsIncluded.FirstOrDefault(r => r.User == user && r.Level == level);

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

            this.WriteEnsuringStatistics(level, () =>
            {
                this.RateLevelRelations.Add(rating);
                this.RecalculateLevelRatingStatisticsInternal(level);
            });
            return true;
        }

        this.WriteEnsuringStatistics(level, () =>
        {
            rating.RatingType = type;
            rating.Timestamp = this._time.Now;
            this.RecalculateLevelRatingStatisticsInternal(level);
        });

        return true;
    }
    
    public int GetTotalRatingsForLevel(GameLevel level, RatingType type, bool includingAuthor = true) =>
        this.RateLevelRelations.Count(r =>
            r.Level == level &&
            r.RatingType == type && 
            (includingAuthor || r.User != level.Publisher));
    
    /// <summary>
    /// Adds a review to the database, deleting any old ones by the user on that level.
    /// </summary>
    /// <param name="review">The review to add</param>
    /// <param name="level">The level the review is for</param>
    /// <param name="user">The user who made the review</param>
    public void AddReviewToLevel(GameReview review, GameLevel level)
    {
        Debug.Assert(review.Publisher != null);
        
        List<GameReview> toRemove = this.GameReviews
            .Where(r => r.Publisher == review.Publisher)
            .Where(r => r.Level == level)
            .ToList();
        if (toRemove.Count > 0)
        {
            this.WriteEnsuringStatistics(review.Publisher, level, () =>
            {
                foreach (GameReview reviewToDelete in toRemove)
                {
                    this.GameReviews.Remove(reviewToDelete);
                    level.Statistics!.ReviewCount--;
                    review.Publisher.Statistics!.ReviewCount--;
                }
            });
        }

        this.WriteEnsuringStatistics(review.Publisher, level, () =>
        {
            this.GameReviews.Add(review);
            level.Statistics!.ReviewCount++;
            review.Publisher.Statistics!.ReviewCount++;
        });
    }
    
    public void DeleteReviewsPostedByUser(GameUser user)
    {
        IEnumerable<GameReview> reviews = this.GameReviews
            .Include(r => r.Level)
            .Include(r => r.Level.Statistics)
            .Include(r => r.Publisher)
            .Include(r => r.Publisher.Statistics)
            .Where(s => s.Publisher == user);
        
        this.Write(() =>
        {
            foreach (GameReview review in reviews)
            {
                this.CalculateLevelStatisticsIfNotPresent(review.Level);
                this.CalculateUserStatisticsIfNotPresent(review.Publisher);
                
                this.GameReviews.Remove(review);
                review.Level.Statistics!.ReviewCount--;
                review.Publisher.Statistics!.ReviewCount--;

                this.MarkLevelStatisticsDirty(review.Level);
                this.MarkUserStatisticsDirty(review.Publisher);
            }
        });
    }

    public DatabaseList<GameReview> GetReviewsByUser(GameUser user, int count, int skip)
        => new(this.GameReviewsIncluded
            .Where(r => r.Publisher == user)
            .OrderByDescending(r => r.PostedAt), skip, count);

    public int GetTotalReviewsByUser(GameUser user)
        => this.GameReviews.Count(r => r.Publisher == user);
    
    public GameReview? GetReviewByLevelAndUser(GameLevel level, GameUser user)
    {
        return this.GameReviewsIncluded
            .Where(r => r.Level == level)
            .AsEnumerableIfRealm()
            .FirstOrDefault(r => r.Publisher.UserId == user.UserId);
    }

    public DatabaseList<GameReview> GetReviewsForLevel(GameLevel level, int count, int skip)
        => new(this.GameReviewsIncluded
            .Where(r => r.Level == level)
            .AsEnumerable()
            // Sort reviews from most liked to most disliked
            .OrderByDescending(this.GetRawRatingForReview), skip, count);
    
    public int GetTotalReviewsForLevel(GameLevel level)
        => this.GameReviews.Count(r => r.Level == level);

    #endregion

    #region Playing
    
    private IQueryable<PlayLevelRelation> PlayLevelRelationsIncluded => this.PlayLevelRelations
        .Include(r => r.User)
        .Include(r => r.Level)
        .Include(r => r.Level.Statistics)
        .Include(r => r.Level.Publisher);
    
    private IQueryable<UniquePlayLevelRelation> UniquePlayLevelRelationsIncluded => this.UniquePlayLevelRelations
        .Include(r => r.User)
        .Include(r => r.Level)
        .Include(r => r.Level.Statistics)
        .Include(r => r.Level.Publisher);
    
    public void PlayLevel(GameLevel level, GameUser user, int count)
    {
        PlayLevelRelation relation = new()
        {
            Level = level,
            User = user,
            Timestamp = this._time.Now,
            Count = count,
        };
        
        UniquePlayLevelRelation? uniqueRelation = this.UniquePlayLevelRelations
            .FirstOrDefault(r => r.Level == level && r.User == user);
        
        this.WriteEnsuringStatistics(level, () =>
        {
            this.PlayLevelRelations.Add(relation);
            
            level.Statistics!.PlayCount++;
            // If the user hasn't played the level before, then add a unique relation too
            if (uniqueRelation == null)
            {
                this.UniquePlayLevelRelations.Add(new UniquePlayLevelRelation
                {
                    Level = level,
                    User = user,
                    Timestamp = this._time.Now,
                });

                this.CreateLevelPlayEvent(user, level);

                level.Statistics!.UniquePlayCount++;
                if(user != level.Publisher)
                    level.Statistics!.UniquePlayCountExcludingPublisher++;
            }
        });
    }

    public bool HasUserPlayedLevel(GameLevel level, GameUser user) =>
        this.UniquePlayLevelRelations
            .FirstOrDefault(r => r.Level == level && r.User == user) != null;
    
    public IEnumerable<PlayLevelRelation> GetAllPlaysForLevel(GameLevel level) =>
        this.PlayLevelRelationsIncluded.Where(r => r.Level == level);
    
    public IEnumerable<PlayLevelRelation> GetAllPlaysForLevelByUser(GameLevel level, GameUser user) =>
        this.PlayLevelRelationsIncluded.Where(r => r.Level == level && r.User == user);
    
    public int GetTotalPlaysForLevel(GameLevel level) =>
        this.GetAllPlaysForLevel(level).Sum(playLevelRelation => playLevelRelation.Count);
    
    public int GetTotalPlaysForLevelByUser(GameLevel level, GameUser user) =>
        this.GetAllPlaysForLevelByUser(level, user).Sum(playLevelRelation => playLevelRelation.Count);
    
    public int GetTotalUniquePlaysForLevel(GameLevel level, bool includingAuthor = true) =>
        this.UniquePlayLevelRelations.Count(r => r.Level == level && (includingAuthor || r.User != level.Publisher));

    public int GetTotalCompletionsForLevel(GameLevel level) =>
        this.GameScores.Count(s => s.Level == level);

    #endregion

    #region Comments

    private IQueryable<ProfileCommentRelation> ProfileCommentRelationsIncluded => this.ProfileCommentRelations
        .Include(r => r.User)
        .Include(r => r.Comment)
        .Include(r => r.Comment.Author)
        .Include(r => r.Comment.Profile);

    private IQueryable<LevelCommentRelation> LevelCommentRelationsIncluded => this.LevelCommentRelations
        .Include(r => r.User)
        .Include(r => r.Comment)
        .Include(r => r.Comment.Author)
        .Include(r => r.Comment.Level);

    private ProfileCommentRelation? GetProfileCommentRelationByUser(GameProfileComment comment, GameUser user) => this.ProfileCommentRelationsIncluded
        .FirstOrDefault(r => r.Comment == comment && r.User == user);
    
    private LevelCommentRelation? GetLevelCommentRelationByUser(GameLevelComment comment, GameUser user) => this.LevelCommentRelationsIncluded
        .FirstOrDefault(r => r.Comment == comment && r.User == user);
    
    /// <summary>
    /// Get a user's rating on a particular profile comment.
    /// A null return value means a user has not set a rating.
    /// </summary>
    /// <param name="comment">The comment to check</param>
    /// <param name="user">The user to check</param>
    /// <returns>The rating if found</returns>
    [Pure]
    public RatingType? GetProfileCommentRatingByUser(GameProfileComment comment, GameUser user) 
        => this.GetProfileCommentRelationByUser(comment, user)?.RatingType;
    
    /// <summary>
    /// Get a user's rating on a particular level comment.
    /// A null return value means a user has not set a rating.
    /// </summary>
    /// <param name="comment">The comment to check</param>
    /// <param name="user">The user to check</param>
    /// <returns>The rating if found</returns>
    [Pure]
    public RatingType? GetLevelCommentRatingByUser(GameLevelComment comment, GameUser user) 
        => this.GetLevelCommentRelationByUser(comment, user)?.RatingType;
    
    public int GetTotalRatingsForProfileComment(GameProfileComment comment, RatingType type) =>
        this.ProfileCommentRelations.Count(r => r.Comment == comment && r.RatingType == type);
    
    public int GetTotalRatingsForLevelComment(GameLevelComment comment, RatingType type) =>
        this.LevelCommentRelations.Count(r => r.Comment == comment && r.RatingType == type);

    private bool RateComment<TComment, TCommentRelation>(GameUser user, TComment comment, RatingType ratingType, DbSet<TCommentRelation> list)
        where TComment : class, IGameComment
        where TCommentRelation : class, ICommentRelation<TComment>, new()
    {
        if (ratingType == RatingType.Neutral)
            return false;
        
        TCommentRelation? relation = list.FirstOrDefault(r => r.Comment == comment && r.User == user);
        
        if (relation == null)
        {
            relation = new TCommentRelation
            {
                User = user,
                Comment = comment,
                Timestamp = this._time.Now,
                RatingType = ratingType,
            };
            
            this.Write(() =>
            {
                list.Add(relation);
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
    
    public bool RateProfileComment(GameUser user, GameProfileComment comment, RatingType ratingType)
        => this.RateComment(user, comment, ratingType, this.ProfileCommentRelations);

    public bool RateLevelComment(GameUser user, GameLevelComment comment, RatingType ratingType)
        => this.RateComment(user, comment, ratingType, this.LevelCommentRelations);

    #endregion

    #region Tags

    private IQueryable<TagLevelRelation> TagLevelRelationsIncluded => this.TagLevelRelations
        .Include(r => r.User)
        .Include(r => r.Level)
        .Include(r => r.Level.Statistics)
        .Include(r => r.Level.Publisher);

    public void AddTagRelation(GameUser user, GameLevel level, Tag tag)
    {
        this.Write(() =>
        {
            // Remove any old tags from this user on this level
            this.TagLevelRelations.RemoveRange(this.TagLevelRelations.Where(t => t.User == user && t.Level == level));
            
            this.TagLevelRelations.Add(new TagLevelRelation
            {
                Tag = tag,
                User = user,
                Level = level,
                Timestamp = this._time.Now,
            });
        });
    }

    public IEnumerable<TagLevelRelation> GetTagsForLevel(GameLevel level)
    {
        IQueryable<TagLevelRelation> levelTags = this.TagLevelRelationsIncluded.Where(t => t.Level == level);
 
        IEnumerable<TagLevelRelation> tags = levelTags
            .AsEnumerableIfRealm() // TODO: optimize for postgres when realm is deleted
            .GroupBy(t => t.Tag).Select(g => g.First())
            // ^ is equivalent to .DistinctBy(t => t._Tag)
            .AsEnumerable()
            .OrderByDescending(t => levelTags.Count(levelTag => levelTag.Tag == t.Tag));

        return tags;
    }
    
    #endregion
}