namespace Refresh.Database.Models.Pins;

/// <summary>
/// The progress types of pins which have to either be awarded manually by the server,
/// or be returned/used elsewhere (e.g. LBP3 challenges)
/// </summary>
public enum ServerPins : long
{
    // Level Leaderboards
    TopFourthOfXStoryLevelsWithOver50Scores = 3394094772,
    TopFourthOfXCommunityLevelsWithOver50Scores = 1700253570, 
    TopXOfAnyStoryLevelWithOver50Scores = 191183438,
    TopXOfAnyCommunityLevelWithOver50Scores = 2033315234,

    // Website
    SignIntoWebsite = 2691148325,
    HeartPlayerOnWebsite = 1965011384,
    QueueLevelOnWebsite = 2833810997,

    // LBP3 challenges
    OverLineLbp3ChallengeMedal = 3003874881,
    OverLineLbp3ChallengeRanking = 2922567456,

    PixelPaceLbp3ChallengeMedal = 282407472,
    PixelPaceLbp3ChallengeRanking = 3340696069,

    RabbitBoxingLbp3ChallengeMedal = 2529088759,
    RabbitBoxingLbp3ChallengeRanking = 958144818,

    FloatyFluidLbp3ChallengeMedal = 183892581,
    FloatyFluidLbp3ChallengeRanking = 3442917932,

    ToggleIslandLbp3ChallengeMedal = 315245769,
    ToggleIslandLbp3ChallengeRanking = 443310584,

    SpaceDodgeballLbp3ChallengeMedal = 144212050,
    SpaceDodgeballLbp3ChallengeRanking = 2123417147,

    InvisibleMazeLbp3ChallengeMedal = 249569175,
    InvisibleMazeLbp3ChallengeRanking = 1943114258,

    HoverboardLbp3ChallengeMedal = 3478661003,
    HoverboardLbp3ChallengeRanking = 592022798,

    WhoopTowerLbp3ChallengeMedal = 216730878,
    WhoopTowerLbp3ChallengeRanking = 545532447,

    SwoopPanelsLbp3ChallengeMedal = 2054302637,
    SwoopPanelsLbp3ChallengeRanking = 3288689476,

    PinballLbp3ChallengeMedal = 618998172,
    PinballLbp3ChallengeRanking = 4087839785,

    TieSkipLbp3ChallengeMedal = 3953447125,
    TieSkipLbp3ChallengeRanking = 2556445436,

    JokerLbp3ChallengeMedal = 1093784294,
    JokerLbp3ChallengeRanking = 1757295127,

    CherryShooterLbp3ChallengeMedal = 1568570416,
    CherryShooterLbp3ChallengeRanking = 3721717765,
}