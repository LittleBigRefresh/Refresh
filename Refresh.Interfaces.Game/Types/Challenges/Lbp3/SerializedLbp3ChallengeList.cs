using System.Xml.Serialization;
using Refresh.Database.Models.Pins;

namespace Refresh.Interfaces.Game.Types.Challenges.Lbp3;

#nullable disable

[XmlRoot("Challenge_header")]
public class SerializedLbp3ChallengeList
{
    [XmlElement("Total_challenges")]
    public int TotalChallenges { get; set; }
    
    /// <summary>
    /// Timestamp is stored as a unix epoch in microseconds, is equal to the very last challenge's end date
    /// </summary>
    [XmlElement("Challenge_End_Date")]
    public ulong EndTime { get; set; }
    
    /// <summary>
    /// Percentage required to get bronze, stored as a float 0-1
    /// </summary>
    [XmlElement("Challenge_Top_Rank_Bronze_Range")]
    public float BronzeRankPercentage { get; set; }
    
    /// <summary>
    /// Percentage required to get silver, stored as a float 0-1
    /// </summary>
    [XmlElement("Challenge_Top_Rank_Silver_Range")]
    public float SilverRankPercentage { get; set; }
    
    /// <summary>
    /// Percentage required to get gold, stored as a float 0-1
    /// </summary>
    [XmlElement("Challenge_Top_Rank_Gold_Range")]
    public float GoldRankPercentage { get; set; }
    
    /// <summary>
    /// Cycle time stored as a unix epoch in microseconds
    /// </summary>
    [XmlElement("Challenge_CycleTime")]
    public ulong CycleTime { get; set; }
    
    // ReSharper disable once IdentifierTypo
    [XmlElement("item_data")]
    public List<SerializedLbp3Challenge> Challenges { get; set; }

    [XmlIgnore] private const ulong FromSecondsFactor = 1_000_000; // Factor to multiply with to convert a unix timestamp from seconds to microseconds
    [XmlIgnore] private const ulong StartTimestamp = 1762872698 * FromSecondsFactor;
    [XmlIgnore] private const ulong Duration = 259200 * FromSecondsFactor; // 3 days

    /// <returns>
    /// A config which is loosely based off of the official server's config, but the timestamps
    /// start from 11/11/2025 3:51:38 PM and every challenge has a duration/period of 3 days.
    /// LBP3 doesn't care if any timestamps are in the past, and simply wraps the challenge periods in that case.
    /// </returns>
    public static SerializedLbp3ChallengeList FromDefault()
    {
        return new()
        {
            TotalChallenges = 14,
            EndTime = StartTimestamp + 14 * Duration,
            BronzeRankPercentage = 0.51f,
            SilverRankPercentage = 0.26f,
            GoldRankPercentage = 0.11f,
            CycleTime = Duration,
            Challenges =
            [
                new()
                {
                    Id = 0,
                    StartTime = StartTimestamp + 0 * Duration,
                    EndTime = StartTimestamp + 1 * Duration,
                    LamsDescriptionId = "CHALLENGE_NEWTONBOUNCE_DESC",
                    LamsTitleId = "CHALLENGE_NEWTONBOUNCE_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.NewtonLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.NewtonLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet3",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1085260,
                    PhotoId = 1112639,
                },
                new()
                {
                    Id = 1,
                    StartTime = StartTimestamp + 1 * Duration,
                    EndTime = StartTimestamp + 2 * Duration,
                    LamsDescriptionId = "CHALLENGE_SCREENCHASE_DESC",
                    LamsTitleId = "CHALLENGE_SCREENCHASE_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.AutoscrollLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.AutoscrollLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet2",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1102387,
                    PhotoId = 1112651,
                },
                new()
                {
                    Id = 2,
                    StartTime = StartTimestamp + 2 * Duration,
                    EndTime = StartTimestamp + 3 * Duration,
                    LamsDescriptionId = "CHALLENGE_RABBITBOXING_DESC",
                    LamsTitleId = "CHALLENGE_RABBITBOXING_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.RabbitBoxerLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.RabbitBoxerLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet2",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1085264,
                    PhotoId = 1112627,
                },
                new()
                {
                    Id = 3,
                    StartTime = StartTimestamp + 3 * Duration,
                    EndTime = StartTimestamp + 4 * Duration,
                    LamsDescriptionId = "CHALLENGE_FLOATYFLUID_DESC",
                    LamsTitleId = "CHALLENGE_FLOATYFLUID_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.FloatyFluidLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.FloatyFluidLbp3ChallengeRanking,
                    Content = "LBPDLCNISBLK0001",
                    ContentName = "SBSP_THEME_PACK_NAME",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1095449,
                    PhotoId = 1112619,
                },
                new()
                {
                    Id = 4,
                    StartTime = StartTimestamp + 4 * Duration,
                    EndTime = StartTimestamp + 5 * Duration,
                    LamsDescriptionId = "CHALLENGE_ISLANDRACE_DESC",
                    LamsTitleId = "CHALLENGE_ISLANDRACE_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.IslandLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.IslandLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1102858,
                    PhotoId = 1112655,
                },
                new()
                {
                    Id = 5,
                    StartTime = StartTimestamp + 5 * Duration,
                    EndTime = StartTimestamp + 6 * Duration,
                    LamsDescriptionId = "CHALLENGE_SPACEDODGING_DESC",
                    LamsTitleId = "CHALLENGE_SPACEDODGING_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.DodgeballLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.DodgeballLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet3",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1085266,
                    PhotoId = 1112667,
                },
                new()
                {
                    Id = 6,
                    StartTime = StartTimestamp + 6 * Duration,
                    EndTime = StartTimestamp + 7 * Duration,
                    LamsDescriptionId = "CHALLENGE_INVISIBLECIRCUIT_DESC",
                    LamsTitleId = "CHALLENGE_INVISIBLECIRCUIT_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.FirePitsLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.FirePitsLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1096814,
                    PhotoId = 1112635,
                },
                new()
                {
                    Id = 7,
                    StartTime = StartTimestamp + 7 * Duration,
                    EndTime = StartTimestamp + 8 * Duration,
                    LamsDescriptionId = "CHALLENGE_HOVERBOARDRAILS_DESC",
                    LamsTitleId = "CHALLENGE_HOVERBOARDRAILS_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.HoverboardLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.HoverboardLbp3ChallengeRanking,
                    Content = "LBPDLCBTTFLK0001",
                    ContentName = "BTTF_LEVEL_KIT_NAME",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1085256,
                    PhotoId = 1112623,
                },
                new()
                {
                    Id = 8,
                    StartTime = StartTimestamp + 8 * Duration,
                    EndTime = StartTimestamp + 9 * Duration,
                    LamsDescriptionId = "CHALLENGE_TOWERBOOST_DESC",
                    LamsTitleId = "CHALLENGE_TOWERBOOST_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.DaVinkiLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.DaVinkiLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet2",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1092504,
                    PhotoId = 1112671,
                },
                new()
                {
                    Id = 9,
                    StartTime = StartTimestamp + 9 * Duration,
                    EndTime = StartTimestamp + 10 * Duration,
                    LamsDescriptionId = "CHALLENGE_SWOOPPANELS_DESC",
                    LamsTitleId = "CHALLENGE_SWOOPPANELS_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.SwoopPanelsLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.SwoopPanelsLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet2",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1085268,
                    PhotoId = 1112643,
                },
                new()
                {
                    Id = 10,
                    StartTime = StartTimestamp + 10 * Duration,
                    EndTime = StartTimestamp + 11 * Duration,
                    LamsDescriptionId = "CHALLENGE_PINBALLCRYPTS_DESC",
                    LamsTitleId = "CHALLENGE_PINBALLCRYPTS_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.PinballLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.PinballLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet3",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1085262,
                    PhotoId = 1112647,
                },
                new()
                {
                    Id = 11,
                    StartTime = StartTimestamp + 11 * Duration,
                    EndTime = StartTimestamp + 12 * Duration,
                    LamsDescriptionId = "CHALLENGE_TIEHOP_DESC",
                    LamsTitleId = "CHALLENGE_TIEHOP_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.TieSkipLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.TieSkipLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1092367,
                    PhotoId = 1112659,
                },
                new()
                {
                    Id = 12,
                    StartTime = StartTimestamp + 12 * Duration,
                    EndTime = StartTimestamp + 13 * Duration,
                    LamsDescriptionId = "CHALLENGE_JOKERFUNHOUSE_DESC",
                    LamsTitleId = "CHALLENGE_JOKERFUNHOUSE_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.JonklerLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.JonklerLbp3ChallengeRanking,
                    Content = "LBPDLCWBDCLK0001",
                    ContentName = "DCCOMICS_THEME_NAME",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1085258,
                    PhotoId = 1112631,
                },
                new()
                {
                    Id = 13,
                    StartTime = StartTimestamp + 13 * Duration,
                    EndTime = StartTimestamp + 14 * Duration,
                    LamsDescriptionId = "CHALLENGE_DINERSHOOTING_DESC",
                    LamsTitleId = "CHALLENGE_DINERSHOOTING_NAME",
                    ScoreMedalPinProgressType = (long)ServerPins.CherryShooterLbp3ChallengeMedal,
                    ScoreRankingPinProgressType = (long)ServerPins.CherryShooterLbp3ChallengeRanking,
                    ContentName = "TG_LittleBigPlanet3",
                    PlanetUser = "qd3c781a5a6-GBen",
                    PlanetId = 1085254,
                    PhotoId = 1112663,
                },
            ],
        };
    }
}