using System.Xml.Serialization;

namespace Refresh.Database.Models.Levels;

public enum GameSkillRewardCondition
{
    [XmlEnum("score")]
    Score = 0,
    [XmlEnum("time")]
    Time = 1,
    [XmlEnum("lives")]
    Lives = 2,
}