using Refresh.GameServer.Types.Challenges.LbpHub;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Challenges
{
    #region Challenge

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
            // TODO: Command and ApiV3/Website toggle to override the received ammount of days until expiration with a custom value (0 for no expiration at all)
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

    public void RemoveChallengesByUser(GameUser user)
    {
        this.Write(() => {
            // Remove Scores on challenges by the user
            this.GameChallengeScores.RemoveRange(s => s.Challenge.Publisher == user);
        
            // Remove Challenges by the user
            this.GameChallenges.RemoveRange(c => c.Publisher == user);
        });
    }

    public void RemoveChallengesForLevel(GameLevel level)
    {
        this.Write(() => {
            // Remove Scores on challenges in the level
            this.GameChallengeScores.RemoveRange(s => s.Challenge.Level == level);

            // Remove Challenges in the level
            this.GameChallenges.RemoveRange(c => c.Level == level);
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

    public IEnumerable<GameChallenge> GetChallenges(string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges, filter);

    public IEnumerable<GameChallenge> GetChallengesNotByUser(GameUser user, string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher != user), filter); 

    public IEnumerable<GameChallenge> GetChallengesByUser(GameUser user, string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher == user), filter);

    public IEnumerable<GameChallenge> GetChallengesForLevel(GameLevel level, string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Level == level), filter);

    #endregion

    #region Score

    public GameChallengeScore CreateChallengeScore(SerializedChallengeAttempt attempt, GameChallenge challenge, GameUser user, long time)
    {
        // Get the first score for this challenge
        GameChallengeScore? firstScore = this.GetFirstScoreForChallenge(challenge);
        bool newHighScore = true;

        // Skip this step if there is no first score (and therefore no scores at all) for this challenge yet.
        if (firstScore != null)
        {
            // Get all scores for this challenge by the user whose ghost hashes are not null
            IEnumerable<GameChallengeScore> otherScores = 
                this.GameChallengeScores.Where(s => s.Challenge == challenge && s.Publisher == user && s.GhostHash != null);

            this.Write(() => {
                foreach (GameChallengeScore otherScore in otherScores)
                {
                    // If the current score is not beaten by the new score (new score is lesser than current score),
                    // don't touch the current score's GhostHash
                    if (attempt.Score < otherScore.Score)
                    {
                        newHighScore = false;
                        continue;
                    }

                    // If the current score is the first score for this challenge, don't touch its GhostHash
                    if (otherScore.Equals(firstScore))
                        continue;

                    // Since the current score is lower than the new score and also not the first score,
                    // set its ghost hash to null since it is not needed anymore
                    otherScore.GhostHash = null;
                }
            });
        }

        // Create new score
        GameChallengeScore score = new()
        {
            Challenge = challenge,
            Publisher = user,
            Score = attempt.Score,
            GhostHash = newHighScore ? attempt.GhostHash : null,
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

    public void RemoveChallengeScoresByUser(GameUser user)
    {
        this.Write(() => 
        {
            this.GameChallengeScores.RemoveRange(s => s.Publisher == user);
        });
    }

    public bool DoesChallengeHaveScores(GameChallenge challenge)
        => this.GameChallengeScores.Any(s => s.Challenge == challenge);

    public GameChallengeScore? GetFirstScoreForChallenge(GameChallenge challenge)
        => this.GameChallengeScores.Where(s => s.Challenge == challenge && s.GhostHash != null)
            // Ordering like this ensures we actually do get the first (oldest) score for the challenge
            .OrderBy(s => s.PublishDate)
            .FirstOrDefault();

    public GameChallengeScoreWithRank? GetRankedFirstScoreForChallenge(GameChallenge challenge)
    {
        // We need the high scores to be able to get the first score's rank among them
        IEnumerable<GameChallengeScore> highScores = this.GameChallengeScores.Where(s => s.Challenge == challenge && s.GhostHash != null);

        // Ordering like this ensures we actually do get the first (oldest) score for the challenge
        GameChallengeScore? firstScore = highScores
            .OrderBy(s => s.PublishDate)
            .FirstOrDefault();
        if (firstScore == null) return null;

        return new(firstScore, highScores.OrderByDescending(s => s.Score).ToList().IndexOf(firstScore) + 1);
    }

    public GameChallengeScore? GetHighScoreByUserForChallenge(GameChallenge challenge, GameUser user)
        => this.GameChallengeScores.Where(s => s.Challenge == challenge && s.GhostHash != null)
            // Ordering like this ensures the high score we get is actually the (newest) high score of the user
            .OrderByDescending(s => s.PublishDate)
            .FirstOrDefault(s => s.Publisher == user);

    public GameChallengeScoreWithRank? GetRankedHighScoreByUserForChallenge(GameChallenge challenge, GameUser user)
    {
        // We need the other high scores to be able to get the wanted high score's rank among them
        IEnumerable<GameChallengeScore> highScores = this.GameChallengeScores.Where(s => s.Challenge == challenge && s.GhostHash != null);

        // Ordering like this ensures the high score we get is actually the (newest) high score of the user
        GameChallengeScore? usersHighScore = highScores
            .OrderByDescending(s => s.PublishDate)
            .FirstOrDefault(s => s.Publisher.UserId == user.UserId);
        if (usersHighScore == null) return null;

        return new(usersHighScore, highScores.OrderByDescending(s => s.Score).ToList().IndexOf(usersHighScore) + 1);
    }

    /// <summary>
    /// Returns whether a user has "cleared" a challenge (how often they have beaten the challenge's first score).
    /// </summary>
    public bool HasUserClearedChallenge(GameChallenge challenge, GameUser user)
    {
        GameChallengeScore? firstScore = this.GetFirstScoreForChallenge(challenge);

        // If there is no first score, there are no scores at all
        if (firstScore == null) return false;

        return this.GameChallengeScores.Any(s => s.Challenge == challenge && s.Publisher == user && s.Score >= firstScore.Score);
    }

    public IEnumerable<GameChallengeScoreWithRank> GetRankedHighScoresForChallenge(GameChallenge challenge)
    {
        IEnumerable<GameChallengeScore> highScores = this.GameChallengeScores
            .Where(s => s.Challenge == challenge && s.GhostHash != null)
            .OrderByDescending(s => s.Score);

        return highScores.Select((s, i) => new GameChallengeScoreWithRank(s, i));
    }

    public IEnumerable<GameChallengeScoreWithRank> GetRankedHighScoresByUsersMutualsForChallenge(GameChallenge challenge, GameUser user)
    {
        IEnumerable<GameUser> mutuals = this.GetUsersMutuals(user);

        IEnumerable<GameChallengeScore> mutualHighScores = this.GameChallengeScores
            .Where(s => s.Challenge == challenge && s.GhostHash != null)
            .AsEnumerable()
            .Where(s => mutuals.Contains(s.Publisher))
            .OrderByDescending(s => s.Score);

        return mutualHighScores.Select((s, i) => new GameChallengeScoreWithRank(s, i));
    }

    /// <summary>
    /// Returns the given score aswell as the scores "around" it depending on the given count aswell as their ranks among the given score's challenge's high score.
    /// If the given score is place 1, the scores below it will be returned; if it is last place, the scores above it will be returned.
    /// The given count must be odd and greater than 0.
    /// </summary>
    /// <seealso cref="GetRankedScoresAroundScore"/>
    public IEnumerable<GameChallengeScoreWithRank> GetRankedHighScoresAroundChallengeScore(GameChallengeScore score, int count)
    {
        if (count <= 0 || count % 2 != 1) 
            throw new ArgumentException("The number of scores must be odd and greater than 0.", nameof(count));

        IEnumerable<GameChallengeScore> highScores = this.GameChallengeScores
            .Where(s => s.Challenge == score.Challenge && s.GhostHash != null)
            .OrderByDescending(s => s.Score);

        // If the given score is the highest score, take the first 3 scores
        if (highScores.First().Equals(score))
            return highScores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1)).Take(count);

        // If the given score is the lowest score, take the last 3 scores
        else if (highScores.Last().Equals(score))
            return highScores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1)).TakeLast(count);

        // Else return the given score with other scores around it
        else
            return highScores
                .Select((s, i) => new GameChallengeScoreWithRank(s, i + 1))
                .Skip(Math.Min(highScores.Count(), highScores.ToList().IndexOf(score) - count / 2)) // center user's score around other scores
                .Take(count)
                .AsEnumerable();
    }

    #endregion
}