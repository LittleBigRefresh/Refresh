namespace Refresh.GameServer.Time;

public interface IDateTimeProvider
{
    public long TimestampMilliseconds { get; }
    public long TimestampSeconds { get; }
    /// <summary>
    /// The earliest acceptable date, in unix seconds
    /// </summary>
    public long EarliestDate { get; }
    public DateTimeOffset Now { get; }
}