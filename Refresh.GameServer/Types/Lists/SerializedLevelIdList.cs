using System.Xml.Serialization;

[XmlRoot("levels")]
[XmlType("levels")]
public class SerializedLevelIdList
{
    public SerializedLevelIdList() {}
    public SerializedLevelIdList(List<int> levelIds)
    {
        this.LevelIds = levelIds;
    }

    [XmlElement("level_id")] public List<int> LevelIds = [];
}