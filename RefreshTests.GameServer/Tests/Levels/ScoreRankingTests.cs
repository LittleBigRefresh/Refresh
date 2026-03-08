using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Levels;

namespace RefreshTests.GameServer.Tests.Levels;

public class ScoreRankingTests : GameServerTest
{
    [Test]
    public void ScoreRanksRecalculatedOnScoreSubmission()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        context.FillLeaderboard(level, 10, 1); // scores from 0 - 9

        DatabaseScoreList scoreList = context.Database.GetTopScoresForLevel(level, 100, 0, 1);
        Assert.That(scoreList.Items.Count(), Is.EqualTo(10));

        int index = 1;
        foreach (ScoreWithRank score in scoreList.Items)
        {
            Assert.That(score.rank, Is.EqualTo(index));
            Assert.That(score.score.Rank, Is.EqualTo(index));
            index++;
        }

        // Now submit our mediocre score (shares a rank with one of the previous scores)
        GameScore ownScore1 = context.SubmitScore(6, 1, level, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(ownScore1.Rank, Is.EqualTo(4));
        context.Database.Refresh();

        scoreList = context.Database.GetTopScoresForLevel(level, 100, 0, 1, user: user);
        Assert.That(scoreList.Items.Count(), Is.EqualTo(11));

        // User has exactly 1 score which is rank 4. Also assert the separate OwnScore attribute.
        Assert.That(scoreList.Items.Count(s => s.score.PublisherId == user.UserId && s.rank == 4 && s.score.Rank == 4), Is.EqualTo(1));
        Assert.That(scoreList.OwnScore, Is.Not.Null);
        Assert.That(scoreList.OwnScore!.rank, Is.EqualTo(4));
        Assert.That(scoreList.OwnScore.score.Rank, Is.EqualTo(4));

        // Don't iterate the list again as that would require somehow special-casing the 2 scores which share a rank.
        // Instead just do some other checks.
        Assert.That(scoreList.Items.First().rank, Is.EqualTo(1));
        Assert.That(scoreList.Items.First().score.Rank, Is.EqualTo(1));
        Assert.That(scoreList.Items.First().score.Score, Is.EqualTo(9));

        Assert.That(scoreList.Items.Last().rank, Is.EqualTo(10)); // not 11
        Assert.That(scoreList.Items.Last().score.Rank, Is.EqualTo(10));
        Assert.That(scoreList.Items.Last().score.Score, Is.EqualTo(0));

        Assert.That(scoreList.Items.Count(s => s.rank == 4), Is.EqualTo(2)); // our and another's score
        Assert.That(scoreList.Items.Count(s => s.score.Rank == 4), Is.EqualTo(2));
        Assert.That(scoreList.Items.Count(s => s.score.Score == 6), Is.EqualTo(2));
    }

    [Test]
    public void RankOfOwnOvertakenScoreGetsReset()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        GameScore ownScore1 = context.SubmitScore(6, 1, level, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(ownScore1.Rank, Is.EqualTo(1));

        GameScore ownScore2 = context.SubmitScore(7, 1, level, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(ownScore2.Rank, Is.EqualTo(1));

        GameScore? ownScore1Overtaken = context.Database.GetScoreByObjectId(ownScore1.ScoreId);
        Assert.That(ownScore1Overtaken, Is.Not.Null);
        Assert.That(ownScore1Overtaken!.Rank, Is.EqualTo(0));
    }

    [Test]
    public void ScoreRanksRecalculatedOnSingleScoreDeletion()
    {
        using TestContext context = this.GetServer();
        GameLevel level = context.CreateLevel(context.CreateUser());
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameUser user3 = context.CreateUser();

        // Create a few scores ourselves
        GameScore score1 = context.SubmitScore(300, 1, level, user1, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        GameScore score2 = context.SubmitScore(200, 1, level, user2, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        GameScore score3 = context.SubmitScore(100, 1, level, user3, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(score1.Rank, Is.EqualTo(1));
        Assert.That(score2.Rank, Is.EqualTo(2));
        Assert.That(score3.Rank, Is.EqualTo(3));

        // Delete a score
        context.Database.DeleteScore(score2);
        context.Database.Refresh();
        GameScore? score1Reranked = context.Database.GetScoreByObjectId(score1.ScoreId);
        GameScore? score2Reranked = context.Database.GetScoreByObjectId(score2.ScoreId);
        GameScore? score3Reranked = context.Database.GetScoreByObjectId(score3.ScoreId);

        Assert.That(score1Reranked, Is.Not.Null);
        Assert.That(score1Reranked!.Rank, Is.EqualTo(1));

        Assert.That(score2Reranked, Is.Null);

        Assert.That(score3Reranked, Is.Not.Null);
        Assert.That(score3Reranked!.Rank, Is.EqualTo(2));
    }

    [Test]
    public void ScoreRanksRecalculatedOnScoresByUserDeletion()
    {
        using TestContext context = this.GetServer();
        GameLevel level1 = context.CreateLevel(context.CreateUser());
        GameLevel level2 = context.CreateLevel(context.CreateUser());
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameUser user3 = context.CreateUser();

        // Create a few scores ourselves
        GameScore score1 = context.SubmitScore(300, 1, level1, user1, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        GameScore score2 = context.SubmitScore(200, 1, level1, user2, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        GameScore score3 = context.SubmitScore(100, 1, level1, user3, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(score1.Rank, Is.EqualTo(1));
        Assert.That(score2.Rank, Is.EqualTo(2));
        Assert.That(score3.Rank, Is.EqualTo(3));
        GameScore score4 = context.SubmitScore(600, 1, level2, user1, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        GameScore score5 = context.SubmitScore(700, 1, level2, user2, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        GameScore score6 = context.SubmitScore(20, 1, level2, user3, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        Assert.That(score4.Rank, Is.EqualTo(2));
        Assert.That(score5.Rank, Is.EqualTo(1));
        Assert.That(score6.Rank, Is.EqualTo(3));

        // Delete all scores by user
        context.Database.DeleteScoresSetByUser(user2);
        context.Database.Refresh();
        GameScore? score1Reranked = context.Database.GetScoreByObjectId(score1.ScoreId);
        GameScore? score2Reranked = context.Database.GetScoreByObjectId(score2.ScoreId);
        GameScore? score3Reranked = context.Database.GetScoreByObjectId(score3.ScoreId);

        Assert.That(score1Reranked, Is.Not.Null);
        Assert.That(score1Reranked!.Rank, Is.EqualTo(1));
        Assert.That(score2Reranked, Is.Null);
        Assert.That(score3Reranked, Is.Not.Null);
        Assert.That(score3Reranked!.Rank, Is.EqualTo(2));

        GameScore? score4Reranked = context.Database.GetScoreByObjectId(score4.ScoreId);
        GameScore? score5Reranked = context.Database.GetScoreByObjectId(score5.ScoreId);
        GameScore? score6Reranked = context.Database.GetScoreByObjectId(score6.ScoreId);

        Assert.That(score4Reranked, Is.Not.Null);
        Assert.That(score4Reranked!.Rank, Is.EqualTo(1));
        Assert.That(score5Reranked, Is.Null);
        Assert.That(score6Reranked, Is.Not.Null);
        Assert.That(score6Reranked!.Rank, Is.EqualTo(2));
    }
}