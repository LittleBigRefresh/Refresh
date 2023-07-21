using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Levels.CustomRewards;

[XmlType("customReward")]
public partial class CustomReward : IEmbeddedObject
{
    [XmlAttribute(AttributeName = "id")] public int Id { get; set; }
    [XmlElement("enabled")] public bool Enabled { get; set; }
    [XmlElement("description")] public string? Description { get; set; }
    [XmlElement("amountNeeded")] public float AmountNeeded { get; set; }

    // Realm can't store enums, use recommended workaround
    // ReSharper disable once InconsistentNaming (can't fix due to conflict with EventType)
    // ReSharper disable once MemberCanBePrivate.Global
    internal int _Condition { get; set; }
    [Ignored] [XmlElement("condition")]
    public CustomRewardCondition Condition
    {
        get => (CustomRewardCondition)this._Condition;
        set => this._Condition = (int)value;
    }
}