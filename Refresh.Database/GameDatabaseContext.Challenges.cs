using Refresh.Common.Constants;
using Refresh.Database.Query;
using Refresh.GameServer.Types.Challenges.LbpHub;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Challenges
{
    #region Challenges

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
        
        this.AddSequentialObject(challenge);

        return challenge;
    }

    public void RemoveChallenge(GameChallenge challenge)
    {
        this.Write(() => {
            // Remove Scores
            this.GameChallengeScores.RemoveRange(s => s.Challenge == challenge);
        
            // Remove Challenge
            this.GameChallenges.Remove(challenge);
        });
    }

    public void RemoveChallengesForLevel(GameLevel level)
    {
        this.Write(() => {
            // Realm weirdness, we're forced to iterate over every challenge of the given level to remove their scores
            IEnumerable<GameChallenge> challenges = this.GameChallenges.Where(c => c.Level == level);

            foreach (GameChallenge challenge in challenges)
            {
                this.GameChallengeScores.RemoveRange(s => s.Challenge == challenge);
            }
            this.GameChallenges.RemoveRange(challenges);
        });
    }

    public GameChallenge? GetChallengeById(int challengeId)
        => this.GameChallenges.FirstOrDefault(c => c.ChallengeId == challengeId);
    
    public int GetTotalChallengeCount(string? status)
    {
        DateTimeOffset now = this._time.Now;
        return status switch
        {
            "active" => this.GameChallenges.Count(c => c.ExpirationDate > now),
            "expired" => this.GameChallenges.Count(c => c.ExpirationDate <= now),
            _ => this.GameChallenges.Count(),
        };
    }

    private IEnumerable<GameChallenge> FilterChallengesByStatus(IEnumerable<GameChallenge> challenges, string? status)
    {
        DateTimeOffset now = this._time.Now;
        return status switch
        {
            // Sort by soonest expiration first
            "active" => challenges.Where(c => c.ExpirationDate > now).OrderBy(c => c.ExpirationDate),

            // Sort by most recently expired first
            "expired" => challenges.Where(c => c.ExpirationDate <= now).OrderByDescending(c => c.ExpirationDate),

            // Sort by newest first
            _ => challenges.OrderByDescending(c => c.PublishDate),
        };
    }

    public DatabaseList<GameChallenge> GetChallenges(int skip, int count, string? filter = null)
        => new(this.FilterChallengesByStatus(this.GameChallenges, filter), skip, count);
    
    public DatabaseList<GameChallenge> GetNewestChallenges(int skip, int count, string? filter = null)
        => new(this.FilterChallengesByStatus(this.GameChallenges, filter)
            .OrderByDescending(c => c.PublishDate), skip, count);

    public DatabaseList<GameChallenge> GetChallengesNotByUser(GameUser user, int skip, int count, string? filter = null)
    {
        if (user.Username == SystemUsers.DeletedUserName)
            return new(this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher != null), filter), skip, count);
        else
            return new(this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher != user), filter), skip, count);
    }

    public DatabaseList<GameChallenge> GetChallengesByUser(GameUser user, int skip, int count, string? filter = null)
    {
        if (user.Username == SystemUsers.DeletedUserName)
            return new(this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher == null), filter), skip, count);
        else
            return new(this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher == user), filter), skip, count);
    }

    public DatabaseList<GameChallenge> GetChallengesForLevel(GameLevel level, int skip, int count, string? filter = null)
        => new(this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Level == level), filter), skip, count);

    #endregion

    #region Scores

    public GameChallengeScore CreateChallengeScore(SerializedChallengeAttempt attempt, GameChallenge challenge, GameUser user, long time)
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
        => this.GameChallengeScores.Any(s => s.Challenge == challenge);

    public GameChallengeScoreWithRank? GetRankedChallengeHighScoreByUser(GameChallenge challenge, GameUser user)
    {
        IEnumerable<GameChallengeScoreWithRank> scores = this.GetRankedChallengeHighScoresInternal(challenge);
        return scores.FirstOrDefault(s => s.score.Publisher.UserId == user.UserId);
    }

    public int GetTotalChallengeHighScoreCount(GameChallenge challenge)
        => this.GameChallengeScores
            .Where(s => s.Challenge == challenge)
            .AsEnumerable()
            .DistinctBy(s => s.Publisher)
            .Count();

    private IEnumerable<GameChallengeScore> GetChallengeHighScoresInternal(GameChallenge challenge)
        => this.GameChallengeScores
            .Where(s => s.Challenge == challenge)
            .OrderByDescending(s => s.Score)
            .AsEnumerable()
            .DistinctBy(s => s.Publisher);

    private IEnumerable<GameChallengeScoreWithRank> GetRankedChallengeHighScoresInternal(GameChallenge challenge)
    {
        IEnumerable<GameChallengeScore> highScores = this.GetChallengeHighScoresInternal(challenge);
        return highScores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1));
    }

    public DatabaseList<GameChallengeScoreWithRank> GetRankedChallengeHighScores(GameChallenge challenge, int skip, int count)
        => new(this.GetRankedChallengeHighScoresInternal(challenge), skip, count);
    public DatabaseList<GameChallengeScoreWithRank> GetLowestRankedChallengeHighScores(GameChallenge challenge, int skip, int count)
        => new(this.GetRankedChallengeHighScoresInternal(challenge).OrderBy(s => s.score.Score), skip, count);

    public DatabaseList<GameChallengeScoreWithRank> GetRankedChallengeHighScoresByMutuals(GameChallenge challenge, GameUser user, int skip, int count)
    {
        IEnumerable<GameUser> mutuals = this.GetUsersMutuals(user);

        IEnumerable<GameChallengeScore> mutualHighScores = this.GetChallengeHighScoresInternal(challenge)
            .AsEnumerable()
            .Where(s => mutuals.Contains(s.Publisher));

        return new(mutualHighScores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1)), skip, count);
    }

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