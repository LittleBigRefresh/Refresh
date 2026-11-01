using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Report;

#nullable disable

[XmlRoot("screenElements")]
public class SerializedReportScreenElements
{ 
    [XmlIgnore]
    private List<SerializedReportSlot> InternalSlot { get; } = [];

    [XmlElement("slot")] 
    public SerializedReportSlot[] Slot 
    {
        get => this.InternalSlot.ToArray();
        set 
        {
            this.InternalSlot.Clear();

            if (value == null)
            {
                return;
            }

            foreach (SerializedReportSlot slot in value)
                this.InternalSlot.Add(slot);
        }
    }

    [XmlIgnore]
    private List<SerializedReportPlayer> InternalPlayer { get; } = []; 
    
    [XmlElement("player")] 
    public SerializedReportPlayer[] Player 
    {
        get => this.InternalPlayer.ToArray();
        set 
        {
            this.InternalPlayer.Clear();

            if (value == null)
            {
                return;
            }
            
            foreach (SerializedReportPlayer player in value)
                this.InternalPlayer.Add(player);
        }
    }
}