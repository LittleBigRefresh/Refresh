using System.Xml.Serialization;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Types.UserData.Leaderboard;

[XmlRoot("scoreboards")]
public class SerializedMultiLeaderboardResponse
{
    public SerializedMultiLeaderboardResponse()
    {
        this.Scoreboards = new List<SerializedPlayerLeaderboardResponse>();
    }

    public SerializedMultiLeaderboardResponse(List<SerializedPlayerLeaderboardResponse> scoreboards)
    {
        this.Scoreboards = scoreboards;
    }
    
    [XmlElement("topScores")]
    public List<SerializedPlayerLeaderboardResponse> Scoreboards;

    public static SerializedMultiLeaderboardResponse FromOldList(DatabaseList<GameSubmittedScore> scores)
    {
        SerializedPlayerLeaderboardResponse scoreboard = new()
        {
            FirstRank = 1,
            PlayerCount = 1,
        };

        int i = 0;
        foreach (GameSubmittedScore score in scores.Items)
        {
            scoreboard.Scores.Add(SerializedLeaderboardScore.FromOld(score, i));
            i += 1;
        }
        
        return new SerializedMultiLeaderboardResponse(new List<SerializedPlayerLeaderboardResponse>
        {
            scoreboard,
        });
    }
}

public class SerializedPlayerLeaderboardResponse
{
    public SerializedPlayerLeaderboardResponse()
    {
        this.FirstRank = 1;
        this.PlayerCount = 1;
        this.Scores = new List<SerializedLeaderboardScore>();
    }

    public SerializedPlayerLeaderboardResponse(List<SerializedLeaderboardScore> scores, int playerCount, int firstRank = 1)
    {
        this.Scores = scores;
        this.PlayerCount = playerCount;
        this.FirstRank = firstRank;
    }
    
    [XmlAttribute("firstRank")]
    public int FirstRank { get; set; }
    
    [XmlElement("playRecord")]
    public List<SerializedLeaderboardScore> Scores { get; set; }

    [XmlAttribute("players")]
    public int PlayerCount { get; set; }
}