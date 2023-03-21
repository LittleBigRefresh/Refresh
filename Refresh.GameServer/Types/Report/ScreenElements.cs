using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot(ElementName="screenElements")]
public class ScreenElements : EmbeddedObject 
{ 
    [XmlIgnore]
    private IList<Slot> InternalSlot { get; }

    [XmlElement(ElementName="slot")] 
    public Slot[] Slot 
    {
        get => this.InternalSlot.ToArray();
        set 
        {
            this.InternalSlot.Clear();

            foreach (Slot slot in value)
                this.InternalSlot.Add(slot);
        }
    }

    [XmlIgnore]
    private IList<Player> InternalPlayer { get; } 
    
    [XmlElement(ElementName="player")] 
    public Player[] Player 
    {
        get => this.InternalPlayer.ToArray();
        set 
        {
            this.InternalPlayer.Clear();

            foreach (Player player in value)
                this.InternalPlayer.Add(player);
        }
    }
}