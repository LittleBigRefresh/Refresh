using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.UserData;

public partial class GameUser
{
    [Ignored] [XmlElement("freeSlots")] public int? FreeSlots { get; set; }
    [Ignored] [XmlElement("lbp2FreeSlots")] public int? FreeSlotsLBP2 { get; set; }
    [Ignored] [XmlElement("lbp3FreeSlots")] public int? FreeSlotsLBP3 { get; set; }
    [Ignored] [XmlElement("entitledSlots")] public int? EntitledSlots { get; set; }
    [Ignored] [XmlElement("lbp2EntitledSlots")] public int? EntitledSlotsLBP2 { get; set; }
    [Ignored] [XmlElement("lbp3EntitledSlots")] public int? EntitledSlotsLBP3 { get; set; }
    [Ignored] [XmlElement("lbp1UsedSlots")] public int? UsedSlots { get; set; }
    [Ignored] [XmlElement("lbp2UsedSlots")] public int? UsedSlotsLBP2 { get; set; }
    [Ignored] [XmlElement("lbp3UsedSlots")] public int? UsedSlotsLBP3 { get; set; }
    [Ignored] [XmlElement("lbp2PurchasedSlots")] public int? PurchasedSlotsLBP2 { get; set; }
    [Ignored] [XmlElement("lbp3PurchasedSlots")] public int? PurchasedSlotsLBP3 { get; set; }

    private partial void SerializeSlots()
    {
        this.EntitledSlots = 100;
        this.EntitledSlotsLBP2 = 100;
        this.EntitledSlotsLBP3 = 100;
        
        this.UsedSlots = 0;
        this.UsedSlotsLBP2 = this.PublishedLevels.Count();
        this.UsedSlotsLBP3 = 0;
        
        this.PurchasedSlotsLBP2 = 0;
        this.PurchasedSlotsLBP3 = 0;
        
        this.FreeSlots = 100;
        this.FreeSlotsLBP2 = 100 - this.UsedSlotsLBP2;
        this.FreeSlotsLBP3 = 100;
    }
}