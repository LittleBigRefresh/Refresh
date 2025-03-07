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
            "active" => challenges.Where(c => c.ExpirationDate > now),
            "expired" => challenges.Where(c => c.ExpirationDate <= now),
            _ => challenges,
        };
    }

    public IEnumerable<GameChallenge> GetChallenges(string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges, filter).AsEnumerable();

    public IEnumerable<GameChallenge> GetChallengesNotByUser(GameUser user, string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher != user), filter).AsEnumerable(); 

    public IEnumerable<GameChallenge> GetChallengesByUser(GameUser user, string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Publisher == user), filter).AsEnumerable();

    public IEnumerable<GameChallenge> GetChallengesForLevel(GameLevel level, string? filter = null)
        => this.FilterChallengesByStatus(this.GameChallenges.Where(c => c.Level == level), filter).AsEnumerable();

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

    public GameChallengeScore? GetFirstScoreForChallenge(GameChallenge challenge)
        => this.GameChallengeScores.FirstOrDefault(s => s.Challenge == challenge && s.GhostHash != null);

    public GameChallengeScoreWithRank? GetRankedFirstScoreForChallenge(GameChallenge challenge)
    {
        IEnumerable<GameChallengeScore> scores = this.GameChallengeScores.Where(s => s.Challenge == challenge && s.GhostHash != null);
        GameChallengeScore? score = scores.FirstOrDefault();

        if (score == null) return null;
        return new(score, scores.OrderByDescending(s => s.Score).ToList().IndexOf(score) + 1);
    }

    public GameChallengeScoreWithRank? GetRankedHighScoreByUserForChallenge(GameChallenge challenge, GameUser user)
    {
        IEnumerable<GameChallengeScore> scores = this.GameChallengeScores.Where(s => s.Challenge == challenge && s.GhostHash != null);
        GameChallengeScore? score = scores.LastOrDefault(s => s.Publisher.UserId == user.UserId);
        
        if (score == null) return null;
        return new(score, scores.OrderByDescending(s => s.Score).ToList().IndexOf(score) + 1);
    }

    /// <summary>
    /// Returns how often a user has beaten the first score of a challenge (how often they have "cleared" the challenge).
    /// </summary>
    public int GetTotalChallengeClearsByUser(GameChallenge challenge, GameUser user)
    {
        GameChallengeScore? firstScore = this.GetFirstScoreForChallenge(challenge);
        if (firstScore == null) return 0;
        // both higher and tied scores count as cleared/beaten (incase of a score which can't be beaten by a higher score for some reason)
        return this.GameChallengeScores.Where(s => s.Challenge == challenge && s.Publisher == user && s.Score >= firstScore.Score).Count();
    }

    public IEnumerable<GameChallengeScoreWithRank> GetRankedScoresForChallenge(GameChallenge challenge)
    {
        IEnumerable<GameChallengeScore> scores = this.GameChallengeScores.Where(s => s.Challenge == challenge && s.GhostHash != null)
            .OrderByDescending(s => s.Score)
            .AsEnumerable();
        IEnumerable<GameChallengeScoreWithRank> rankedScores = scores.Select((s, i) => new GameChallengeScoreWithRank(s, i));
        return rankedScores;
    }

    public IEnumerable<GameChallengeScoreWithRank> GetRankedScoresByUsersMutualsForChallenge(GameChallenge challenge, GameUser user)
    {
        IEnumerable<GameUser> mutuals = this.GetUsersMutuals(user);
        IEnumerable<GameChallengeScore> scores = this.GameChallengeScores.Where(s => s.Challenge == challenge && s.GhostHash != null);
        scores = scores.Where(s => mutuals.Contains(s.Publisher))
            .OrderByDescending(s => s.Score);
        IEnumerable<GameChallengeScoreWithRank> rankedScores = scores.Select((s, i) => new GameChallengeScoreWithRank(s, i));
        return rankedScores;
    }

    /// <seealso cref="GetRankedScoresAroundScore"/>
    public IEnumerable<GameChallengeScoreWithRank> GetRankedScoresAroundChallengeScore(GameChallengeScore score, int count)
    {
        if (count <= 0 || count % 2 != 1) 
            throw new ArgumentException("The number of scores must be odd and greater than 0.", nameof(count));

        IEnumerable<GameChallengeScore> scores = this.GameChallengeScores.Where(s => s.Challenge == score.Challenge)
            .OrderByDescending(s => s.Score)
            .AsEnumerable();

        // If the given score is the highest score, take the first 3 scores
        if (scores.First().Equals(score))
            return scores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1)).Take(count);

        // If the given score is the lowest score, take the last 3 scores
        else if (scores.Last().Equals(score))
            return scores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1)).TakeLast(count);

        // Else return the given score with other scores around it
        else
            return scores.Select((s, i) => new GameChallengeScoreWithRank(s, i + 1))
                .Skip(Math.Min(scores.Count(), scores.ToList().IndexOf(score) - count / 2)) // center user's score around other scores
                .Take(count)
                .AsEnumerable();
    }

    #endregion
}