using Refresh.Common.Constants;
using Refresh.GameServer.Types.Challenges.LbpHub;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Challenges
{
    #region Challenges

    public GameChallenge CreateChallenge(SerializedChallenge createInfo, GameLevel level, GameUser user)
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
            Type = (GameChallengeType)createInfo.Criteria[0].Type,
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

    public IEnumerable<GameChallenge> GetChallenges(string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges, filter);

    public IEnumerable<GameChallenge> GetChallengesNotByUser(GameUser user, string? filter = null)
    {
        if (user.Username == SystemUsers.DeletedUserName)
            return this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher != null), filter);
        else
            return this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher != user), filter);
    }

    public IEnumerable<GameChallenge> GetChallengesByUser(GameUser user, string? filter = null)
    {
        if (user.Username == SystemUsers.DeletedUserName)
            return this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher == null), filter);
        else
            return this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher == user), filter);
    } 

    public IEnumerable<GameChallenge> GetChallengesForLevel(GameLevel level, string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Level == level), filter);

    #endregion

    #region Scores

    public GameChallengeScore CreateChallengeScore(SerializedChallengeAttempt attempt, GameChallenge challenge, GameUser user, long time)
    {
        // Create new score
        GameChallengeScore score = new()
        {
            Challenge = challenge,
            Publisher = user,
            Score = attempt.Score,
            GhostHash = attempt.GhostHash,
            Time = time,
            PublishDate = this._time.Now,
        };

        // Add the score and return it
        this.AddSequentialObject(score);
        
        return score;
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

    public GameChallengeScoreWithRank? GetRankedHighScoreByUserForChallenge(GameChallenge challenge, GameUser user)
    {
        // We need the other high scores to be able to get the wanted high score's rank
        IEnumerable<GameChallengeScore> scores = this.GetHighScoresForChallenge(challenge);

        // Ordering like this ensures the high score we get is actually the (newest) high score of the user
        GameChallengeScore? usersScore = scores
            .FirstOrDefault(s => s.Publisher.UserId == user.UserId);

        if (usersScore == null) return null;
        return new(usersScore, scores.OrderByDescending(s => s.Score).ToList().IndexOf(usersScore) + 1);
    }

    private IEnumerable<GameChallengeScore> GetHighScoresForChallenge(GameChallenge challenge, bool showDuplicates = false)
    {
        IEnumerable<GameChallengeScore> scores = this.GameChallengeScores
            .Where(s => s.Challenge == challenge)
            .OrderByDescending(s => s.Score);
        
        if (!showDuplicates)
            scores = scores.DistinctBy(s => s.Publisher);
        
        return scores;
    }

    public IEnumerable<GameChallengeScoreWithRank> GetRankedHighScoresForChallenge(GameChallenge challenge)
    {
        IEnumerable<GameChallengeScore> highScores = this.GetHighScoresForChallenge(challenge);

        return highScores.Select((s, i) => new GameChallengeScoreWithRank(s, i));
    }

    public DatabaseList<GameChallengeScoreWithRank> GetRankedHighScoresForChallenge(GameChallenge challenge, int skip, int count)
        => new(this.GetRankedHighScoresForChallenge(challenge), skip, count);

    public IEnumerable<GameChallengeScoreWithRank> GetRankedHighScoresByUsersMutualsForChallenge(GameChallenge challenge, GameUser user)
    {
        IEnumerable<GameUser> mutuals = this.GetUsersMutuals(user);

        IEnumerable<GameChallengeScore> mutualHighScores = this.GetHighScoresForChallenge(challenge)
            .AsEnumerable()
            .Where(s => mutuals.Contains(s.Publisher))
            .OrderByDescending(s => s.Score);

        return mutualHighScores.Select((s, i) => new GameChallengeScoreWithRank(s, i));
    }

    public DatabaseList<GameChallengeScoreWithRank> GetRankedHighScoresByUsersMutualsForChallenge(GameChallenge challenge, GameUser user, int skip, int count)
        => new(this.GetRankedHighScoresByUsersMutualsForChallenge(challenge, user), skip, count);

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

        IEnumerable<GameChallengeScore> highScores = this.GetHighScoresForChallenge(score.score.Challenge)
            .OrderByDescending(s => s.Score);

        return new
        (
            highScores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1)), 
            Math.Min(highScores.Count(), score.rank - 1 - count / 2), // center user's score around other scores
            count
        ); 
    }

    #endregion
}