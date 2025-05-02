using System.Xml.Serialization;
using Refresh.Database;

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

    public static SerializedMultiLeaderboardResponse FromOld(MultiLeaderboard multiLeaderboard)
    {
        List<SerializedPlayerLeaderboardResponse> leaderboards = new();

        //Iterate over all leaderboards in the list
        foreach ((byte type, DatabaseList<GameSubmittedScore> scores) in multiLeaderboard.Leaderboards)
        {
            SerializedPlayerLeaderboardResponse leaderboard = new()
            {
                FirstRank = 1,
                PlayerCount = type,
            };

            int i = 1;
            foreach (GameSubmittedScore score in scores.Items)
            {
                leaderboard.Scores.Add(SerializedLeaderboardScore.FromOld(score, i));
                i += 1;
            }
            
            leaderboards.Add(leaderboard);
        }
        
        return new SerializedMultiLeaderboardResponse(leaderboards);
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