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
    NewtonLbp3ChallengeMedal = 3003874881,
    NewtonLbp3ChallengeRanking = 2922567456,

    AutoscrollLbp3ChallengeMedal = 282407472,
    AutoscrollLbp3ChallengeRanking = 3340696069,

    RabbitBoxerLbp3ChallengeMedal = 2529088759,
    RabbitBoxerLbp3ChallengeRanking = 958144818,

    FloatyFluidLbp3ChallengeMedal = 183892581,
    FloatyFluidLbp3ChallengeRanking = 3442917932,

    IslandLbp3ChallengeMedal = 315245769,
    IslandLbp3ChallengeRanking = 443310584,

    DodgeballLbp3ChallengeMedal = 144212050,
    DodgeballLbp3ChallengeRanking = 2123417147,

    FirePitsLbp3ChallengeMedal = 249569175,
    FirePitsLbp3ChallengeRanking = 1943114258,

    HoverboardLbp3ChallengeMedal = 3478661003,
    HoverboardLbp3ChallengeRanking = 592022798,

    DaVinkiLbp3ChallengeMedal = 216730878,
    DaVinkiLbp3ChallengeRanking = 545532447,

    SwoopPanelsLbp3ChallengeMedal = 2054302637,
    SwoopPanelsLbp3ChallengeRanking = 3288689476,

    PinballLbp3ChallengeMedal = 618998172,
    PinballLbp3ChallengeRanking = 4087839785,

    TieSkipLbp3ChallengeMedal = 3953447125,
    TieSkipLbp3ChallengeRanking = 2556445436,

    JonklerLbp3ChallengeMedal = 1093784294,
    JonklerLbp3ChallengeRanking = 1757295127,

    CherryShooterLbp3ChallengeMedal = 1568570416,
    CherryShooterLbp3ChallengeRanking = 3721717765,
}