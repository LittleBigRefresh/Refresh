namespace Refresh.Database.Query;

public interface ISerializedChallengeAttempt
{
    long Score { get; }
    string GhostHash { get; }
}