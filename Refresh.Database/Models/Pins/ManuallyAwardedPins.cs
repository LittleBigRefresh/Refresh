namespace Refresh.Database.Models.Pins;

/// <summary>
/// The progress types of pins which either can or have to be awarded manually by the server.
/// </summary>
public enum ManuallyAwardedPins : long
{
    // Level Leaderboards
    TopFourthOfXStoryLevelsWithOver50Scores = 3394094772,
    TopFourthOfXCommunityLevelsWithOver50Scores = 1700253570, 
    TopXOfAnyStoryLevelWithOver50Scores = 191183438,
    TopXOfAnyCommunityLevelWithOver50Scores = 2033315234,

    // Level Rating
    YayXCommunityLevelsWithUnder10Plays = 2778528358,
    YayXCommunityLevels = 1333342859,

    // Website
    SignIntoWebsite = 2691148325,
    HeartPlayerOnWebsite = 1965011384,
    QueueLevelOnWebsite = 2833810997,
}