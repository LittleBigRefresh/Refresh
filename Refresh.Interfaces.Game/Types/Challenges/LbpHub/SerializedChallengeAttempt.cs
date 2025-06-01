using System.Xml.Serialization;
using Refresh.Database.Query;

namespace Refresh.Interfaces.Game.Types.Challenges.LbpHub;

//  <challenge-attempt>
//      <score>9223371991757619189</score>
//      <ghost>b8127d2b7acb3062249022bbce63e81e4f996c52</ghost>
//  </challenge-attempt>
[XmlRoot("challenge-attempt")]
[XmlType("challenge-attempt")]
public class SerializedChallengeAttempt : ISerializedChallengeAttempt
{
    /// <summary>
    /// The publisher's achieved raw score. More always means better here, independent of challenge criteria.
    /// </summary>
    [XmlElement("score")] public long Score { get; set; }
    /// <summary>
    /// The hash of the ghost asset belonging to this score.
    /// </summary>
    [XmlElement("ghost")] public string GhostHash { get; set; } = "";
}