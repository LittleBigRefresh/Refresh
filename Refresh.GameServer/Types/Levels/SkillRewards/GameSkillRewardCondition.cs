using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Levels.SkillRewards;

public enum GameSkillRewardCondition
{
    [XmlEnum("score")]
    Score = 0,
    [XmlEnum("time")]
    Time = 1,
    [XmlEnum("lives")]
    Lives = 2,
}