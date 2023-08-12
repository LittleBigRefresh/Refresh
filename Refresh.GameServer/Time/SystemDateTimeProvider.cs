namespace Refresh.GameServer.Time;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public long TimestampMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public long TimestampSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public DateTimeOffset Now => DateTimeOffset.Now;
}