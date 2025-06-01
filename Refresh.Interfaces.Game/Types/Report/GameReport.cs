using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Report;

#nullable disable

[XmlRoot("griefReport")]
public class GameReport
{
    [XmlIgnore]
    private List<InfoBubble> InternalInfoBubble { get; } = [];
    
    [XmlElement("infoBubble")]
    public InfoBubble[] InfoBubble 
    {
        get => this.InternalInfoBubble.ToArray();
        set 
        {
            this.InternalInfoBubble.Clear();

            if (value == null)
            {
                return;
            }
            
            foreach (InfoBubble infoBubble in value)
                this.InternalInfoBubble.Add(infoBubble);
        }
    }

    [XmlIgnore]
    public GriefReportType Type { get => (GriefReportType)this.InternalType; set => this.InternalType = (int)value; }

    [XmlElement("griefTypeId")]
    private int InternalType { get; set; }

    [XmlElement("marqee")]
    public Marqee Marqee { get; set; }

    [XmlElement("levelOwner")]
    public string LevelOwner { get; set; }

    [XmlElement("initialStateHash")]
    public string InitialStateHash { get; set; }

    [XmlElement("levelType")]
    public string LevelType { get; set; }

    [XmlElement("levelId")]
    public int LevelId { get; set; }
    
    [XmlElement("description")]
    public string Description { get; set; }
    
    [XmlElement("griefStateHash")]
    public string GriefStateHash { get; set; }

    [XmlElement("jpegHash")]
    public string JpegHash { get; set; }

    [XmlIgnore]
    private List<Player> InternalPlayers { get; } = [];

    [XmlElement("player")]
    public Player[] Players 
    {
        get => this.InternalPlayers.ToArray();
        set 
        {
            this.InternalPlayers.Clear();

            if (value == null)
            {
                return;
            }
            
            foreach (Player player in value)
                this.InternalPlayers.Add(player);
        }
    }

    [XmlElement("screenElements")]
    public ScreenElements ScreenElements { get; set; }
}