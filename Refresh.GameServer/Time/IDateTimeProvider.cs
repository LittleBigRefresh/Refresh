namespace Refresh.GameServer.Time;

public interface IDateTimeProvider
{
    public long TimestampMilliseconds { get; }
    public long TimestampSeconds { get; }
    public DateTimeOffset Now { get; }
}