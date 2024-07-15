using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;
using TokenType = Refresh.GameServer.Authentication.TokenType;

namespace RefreshTests.GameServer.Tests.Comments;

public class CommentTests : GameServerTest
{
    public static void RateComment(TestContext context, GameUser user, IGameComment comment, string rateCommentUrl, string getCommentsUrl)
    {
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);


        foreach (RatingType ratingType in new List<RatingType> {RatingType.Neutral, RatingType.Boo, RatingType.Yay})
        {
            for (int i = 0; i < 3; i++) // Rate multiple times to test that duplicate ratings are not added
            {
                // ReSharper disable once RedundantAssignment
                client.PostAsync($"{rateCommentUrl}?commentId={comment.SequentialId}&rating={ratingType.ToDPad()}", null);
            }
        
            HttpResponseMessage response = client.GetAsync(getCommentsUrl).Result;
            SerializedCommentList userComments = response.Content.ReadAsXML<SerializedCommentList>();
            SerializedComment serializedComment = userComments.Items.First();

            int expectedThumbsUp, expectedThumbsDown;
            
            switch (ratingType)
            {
                case RatingType.Neutral:
                    expectedThumbsDown = 0;
                    expectedThumbsUp = 0;
                    break;
                case RatingType.Boo:
                    expectedThumbsDown = 1;
                    expectedThumbsUp = 0;
                    break;
                case RatingType.Yay:
                    expectedThumbsDown = 0;
                    expectedThumbsUp = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Assert.Multiple(() =>
            {
                Assert.That(serializedComment.YourThumb, Is.EqualTo(ratingType.ToDPad()));
                Assert.That(serializedComment.ThumbsUp, Is.EqualTo(expectedThumbsUp));
                Assert.That(serializedComment.ThumbsDown, Is.EqualTo(expectedThumbsDown));
            });
        }
        
        
    }
}