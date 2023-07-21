using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Levels.SkillRewards;

[XmlType("customReward")]
public partial class GameSkillReward : IEmbeddedObject
{
    [XmlAttribute(AttributeName = "id")] public int Id { get; set; }
    [XmlElement("enabled")] public bool Enabled { get; set; }
    [XmlElement("description")] public string? Title { get; set; }
    [XmlElement("amountNeeded")] public float RequiredAmount { get; set; }

    // Realm can't store enums, use recommended workaround
    // ReSharper disable once InconsistentNaming (can't fix due to conflict with ConditionType)
    // ReSharper disable once MemberCanBePrivate.Global
    internal int _ConditionType { get; set; }
    [Ignored] [XmlElement("condition")]
    public GameSkillRewardCondition ConditionType
    {
        get => (GameSkillRewardCondition)this._ConditionType;
        set => this._ConditionType = (int)value;
    }
}