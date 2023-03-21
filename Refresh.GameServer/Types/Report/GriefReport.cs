using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Types.Report;

[XmlRoot(ElementName = "griefReport")]
public class GriefReport : RealmObject, ISequentialId 
{
    [XmlIgnore]
    private IList<InfoBubble> InternalInfoBubble { get; }
    
    [Ignored]
    [XmlElement(ElementName = "infoBubble")]
    public InfoBubble[] InfoBubble 
    {
        get => this.InternalInfoBubble.ToArray();
        set 
        {
            this.InternalInfoBubble.Clear();
            
            foreach (InfoBubble infoBubble in value)
                this.InternalInfoBubble.Add(infoBubble);
        }
    }

    [XmlIgnore]
    public GriefReportType Type { get => (GriefReportType)this.InternalType; set => this.InternalType = (int)value; }

    [XmlElement(ElementName = "griefTypeId")]
    private int InternalType { get; set; }

    [XmlElement(ElementName = "marqee")]
    public Marqee Marqee { get; set; }

    [XmlElement(ElementName = "levelOwner")]
    public string LevelOwner { get; set; }

    [XmlElement(ElementName = "initialStateHash")]
    public string InitialStateHash { get; set; }

    [XmlElement(ElementName = "levelType")]
    public string LevelType { get; set; }

    [XmlElement(ElementName = "levelId")]
    public int LevelId { get; set; }

    [XmlElement(ElementName = "griefStateHash")]
    public string GriefStateHash { get; set; }

    [XmlElement(ElementName = "jpegHash")]
    public string JpegHash { get; set; }

    [XmlIgnore]
    private IList<Player> InternalPlayers { get; }

    [Ignored]
    [XmlElement(ElementName = "player")]
    public Player[] Players 
    {
        get => this.InternalPlayers.ToArray();
        set 
        {
            this.InternalPlayers.Clear();
            
            foreach (Player player in value)
                this.InternalPlayers.Add(player);
        }
    }

    [XmlElement(ElementName = "screenElements")]
    public ScreenElements ScreenElements { get; set; }

    [PrimaryKey]
    public int SequentialId 
    {
        get;
        set;
    }
}