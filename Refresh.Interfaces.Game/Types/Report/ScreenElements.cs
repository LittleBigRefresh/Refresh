using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Report;

#nullable disable

[XmlRoot("screenElements")]
public class ScreenElements
{ 
    [XmlIgnore]
    private List<Slot> InternalSlot { get; } = [];

    [XmlElement("slot")] 
    public Slot[] Slot 
    {
        get => this.InternalSlot.ToArray();
        set 
        {
            this.InternalSlot.Clear();

            if (value == null)
            {
                return;
            }

            foreach (Slot slot in value)
                this.InternalSlot.Add(slot);
        }
    }

    [XmlIgnore]
    private List<Player> InternalPlayer { get; } = []; 
    
    [XmlElement("player")] 
    public Player[] Player 
    {
        get => this.InternalPlayer.ToArray();
        set 
        {
            this.InternalPlayer.Clear();

            if (value == null)
            {
                return;
            }
            
            foreach (Player player in value)
                this.InternalPlayer.Add(player);
        }
    }
}