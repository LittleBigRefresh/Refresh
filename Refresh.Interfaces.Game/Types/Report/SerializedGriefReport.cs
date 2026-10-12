using System.Xml.Serialization;
using Refresh.Database.Models.Reports;

namespace Refresh.Interfaces.Game.Types.Report;

#nullable disable

[XmlRoot("griefReport")]
public class SerializedGriefReport
{
    [XmlIgnore]
    private List<SerializedReportInfoBubble> InternalInfoBubble { get; } = [];
    
    [XmlElement("infoBubble")]
    public SerializedReportInfoBubble[] InfoBubble 
    {
        get => this.InternalInfoBubble.ToArray();
        set 
        {
            this.InternalInfoBubble.Clear();

            if (value == null)
            {
                return;
            }
            
            foreach (SerializedReportInfoBubble infoBubble in value)
                this.InternalInfoBubble.Add(infoBubble);
        }
    }

    [XmlIgnore]
    public GriefReportType Type { get => (GriefReportType)this.InternalType; set => this.InternalType = (int)value; }

    [XmlElement("griefTypeId")]
    public int InternalType { get; set; }

    [XmlElement("marqee")]
    public SerializedReportMarqee Marqee { get; set; }

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
    private List<SerializedReportPlayer> InternalPlayers { get; } = [];

    [XmlElement("player")]
    public SerializedReportPlayer[] Players 
    {
        get => this.InternalPlayers.ToArray();
        set 
        {
            this.InternalPlayers.Clear();

            if (value == null)
            {
                return;
            }
            
            foreach (SerializedReportPlayer player in value)
                this.InternalPlayers.Add(player);
        }
    }

    [XmlElement("screenElements")]
    public SerializedReportScreenElements ScreenElements { get; set; }
}