using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData.Leaderboard;

[XmlRoot("scoreboards")]
public class SerializedLeaderboardResponse
{
    public SerializedLeaderboardResponse() { }

    public SerializedLeaderboardResponse(List<SerializedPlayerLeaderboardResponse> scoreboards)
    {
        this.Scoreboards = scoreboards;
    }
    
    [XmlElement("topScores")]
    public List<SerializedPlayerLeaderboardResponse> Scoreboards;
}

public class SerializedPlayerLeaderboardResponse
{
    public SerializedPlayerLeaderboardResponse() { }

    public SerializedPlayerLeaderboardResponse(List<SerializedLeaderboardScore> scores, int playerCount, int firstRank = 1)
    {
        this.Scores = scores;
        this.PlayerCount = playerCount;
        this.FirstRank = firstRank;
    }
    
    [XmlElement("playRecord")]
    public List<SerializedLeaderboardScore> Scores { get; set; }

    [XmlAttribute("players")]
    public int PlayerCount { get; set; }

    [XmlAttribute("firstRank")]
    public int FirstRank { get; set; }
}