using Refresh.Common.Constants;
using Refresh.Database.Models.Levels.Challenges;
using Refresh.Database.Query;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database;

public partial class GameDatabaseContext // Challenges
{
    #region Challenges

    private IQueryable<GameChallenge> GameChallengesIncluded => this.GameChallenges
        .Include(c => c.Level)
        .Include(c => c.Level.Publisher)
        .Include(c => c.Level.Publisher!.Statistics);

    public GameChallenge CreateChallenge(ICreateChallengeInfo createInfo, GameLevel level, GameUser user)
    {
        DateTimeOffset now = this._time.Now;

        GameChallenge challenge = new()
        {
            Name = createInfo.Name,
            Publisher = user,
            Level = level,
            StartCheckpointUid = createInfo.StartCheckpointUid,
            FinishCheckpointUid = createInfo.FinishCheckpointUid,
            // Take the type of the first (so far always only) criterion in the challenge criteria
            Type = createInfo.CriteriaTypes.First(),
            PublishDate = now,
            LastUpdateDate = now,
            ExpirationDate = now.AddDays(createInfo.ExpiresAt),
        };
        
        this.Write(() =>
        {
            this.GameChallenges.Add(challenge);
        });

        return challenge;
    }

    public void RemoveChallenge(GameChallenge challenge)
    {
        this.Write(() => {
            // Remove Scores
            this.GameChallengeScores.RemoveRange(s => s.ChallengeId == challenge.ChallengeId);
        
            // Remove Challenge
            this.GameChallenges.Remove(challenge);
        });
    }

    public void RemoveChallengesForLevel(GameLevel level)
    {
        this.Write(() => {
            // Realm weirdness, we're forced to iterate over every challenge of the given level to remove their scores
            IEnumerable<GameChallenge> challenges = this.GameChallenges.Where(c => c.LevelId == level.LevelId);

            foreach (GameChallenge challenge in challenges)
            {
                this.GameChallengeScores.RemoveRange(s => s.ChallengeId == challenge.ChallengeId);
            }
            this.GameChallenges.RemoveRange(challenges);
        });
    }

    public GameChallenge? GetChallengeById(int challengeId)
        => this.GameChallengesIncluded.FirstOrDefault(c => c.ChallengeId == challengeId);
    
    public int GetTotalChallengeCount()
    {
        return this.GameChallenges.Count();
    }

    public IEnumerable<GameChallenge> GetNewestChallengesInternal()
        => this.GameChallengesIncluded.OrderByDescending(c => c.PublishDate);

    public DatabaseList<GameChallenge> GetNewestChallenges(int skip, int count)
        => new(this.GetNewestChallengesInternal(), skip, count);

    public DatabaseList<GameChallenge> GetChallengesNotByUser(GameUser user, int skip, int count)
    {
        IEnumerable<GameChallenge> challenges = this.GetNewestChallengesInternal();

        if (user.Username == SystemUsers.DeletedUserName)
            challenges = challenges.Where(c => c.PublisherUserId != null);
        else
            challenges = challenges.Where(c => c.PublisherUserId != user.UserId);

        return new(challenges, skip, count);
    }

    public DatabaseList<GameChallenge> GetChallengesByUser(GameUser user, int skip, int count)
    {
        IEnumerable<GameChallenge> challenges = this.GetNewestChallengesInternal();

        if (user.Username == SystemUsers.DeletedUserName)
            challenges = challenges.Where(c => c.PublisherUserId == null);
        else
            challenges = challenges.Where(c => c.PublisherUserId == user.UserId);

        return new(challenges, skip, count);
    }

    public DatabaseList<GameChallenge> GetChallengesForLevel(GameLevel level, int skip, int count)
        => new(this.GetNewestChallengesInternal().Where(c => c.LevelId == level.LevelId), skip, count);

    #endregion

    #region Scores

    private IQueryable<GameChallengeScore> GameChallengeScoresIncluded => this.GameChallengeScores
        .Include(s => s.Challenge)
        .Include(s => s.Publisher);

    public GameChallengeScore CreateChallengeScore(ISerializedChallengeAttempt attempt, GameChallenge challenge, GameUser user, long time)
    {
        // Notify the previous #1 Score publisher that their score has been overtaken
        GameChallengeScoreWithRank? prevRankOne = this.GetRankedChallengeHighScores(challenge, 0, 1).Items.FirstOrDefault();

        if (prevRankOne != null && // If there already is atleast one score (which is also currently rank 1)
            user.UserId != prevRankOne.score.Publisher.UserId && // That score was uploaded by someone else
            attempt.Score > prevRankOne.score.Score && // The new score is higher than the current #1 score
            prevRankOne.score.Score > 0 // The overtaken score is not 0
        )
        {
            this.AddNotification("Challenge Score overtaken",
                $"Your #1 score on '{challenge.Name}' in '{challenge.Level.Title}' has been overtaken by {user.Username}!",
                prevRankOne.score.Publisher, "medal");
        }

        // Create new score
        GameChallengeScore newScore = new()
        {
            Challenge = challenge,
            Publisher = user,
            Score = attempt.Score,
            GhostHash = attempt.GhostHash,
            Time = time,
            PublishDate = this._time.Now,
        };

        // Add the score and then return it
        this.Write(() =>
        {
            this.GameChallengeScores.Add(newScore);
        });

        return newScore;
    }

    public void RemoveChallengeScore(GameChallengeScore score)
    {
        this.Write(() => 
        {
            this.GameChallengeScores.Remove(score);
        });
    }

    public bool DoesChallengeHaveScores(GameChallenge challenge)
        => this.GameChallengeScores.Any(s => s.ChallengeId == challenge.ChallengeId);

    private IEnumerable<GameChallengeScoreWithRank> GetRankedChallengeHighScoresInternal(GameChallenge challenge)
    {
        IEnumerable<GameChallengeScore> highScores = this.GameChallengeScoresIncluded
            .Where(s => s.ChallengeId == challenge.ChallengeId)
            .OrderByDescending(s => s.Score)
            .ToArray()
            .DistinctBy(s => s.PublisherUserId);

        return highScores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1));
    }

    public GameChallengeScoreWithRank? GetRankedChallengeHighScoreByUser(GameChallenge challenge, GameUser user)
    {
        IEnumerable<GameChallengeScoreWithRank> scores = this.GetRankedChallengeHighScoresInternal(challenge);
        return scores.FirstOrDefault(s => s.score.PublisherUserId == user.UserId);
    }

    public int GetTotalChallengeHighScoreCount(GameChallenge challenge)
        => this.GameChallengeScores
            .Where(s => s.ChallengeId == challenge.ChallengeId)
            .GroupBy(s => s.PublisherUserId)
            .Count();

    public DatabaseList<GameChallengeScoreWithRank> GetRankedChallengeHighScores(GameChallenge challenge, int skip, int count)
        => new(this.GetRankedChallengeHighScoresInternal(challenge), skip, count);

    public DatabaseList<GameChallengeScoreWithRank> GetLowestRankedChallengeHighScores(GameChallenge challenge, int skip, int count)
        => new(this.GetRankedChallengeHighScoresInternal(challenge).OrderBy(s => s.score.Score), skip, count);

    /// <summary>
    /// Returns the given score aswell as the scores "around" it depending on the given count.
    /// Assumes that the rank of the given score is the rank among all high scores of the score's challenge.
    /// The given count must be odd and greater than 0.
    /// </summary>
    /// <seealso cref="GetRankedScoresAroundScore"/>
    public DatabaseList<GameChallengeScoreWithRank> GetRankedHighScoresAroundChallengeScore(GameChallengeScoreWithRank score, int count)
    {
        if (count <= 0 || count % 2 != 1) 
            throw new ArgumentException("The number of scores must be odd and greater than 0.", nameof(count));

        IEnumerable<GameChallengeScoreWithRank> highScores = this.GetRankedChallengeHighScoresInternal(score.score.Challenge);

        return new
        (
            highScores, 
            Math.Min(highScores.Count(), score.rank - 1 - count / 2), // center user's score around other scores
            count
        ); 
    }

    #endregion
}