namespace Refresh.Database.Query;

public interface ISerializedScore
{
    public bool Host { get; set; }
    public byte ScoreType { get; set; }
    public int Score { get; set; }
}