namespace Refresh.Common.Time;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public long TimestampMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public long TimestampSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public long EarliestDate => new DateTimeOffset(2007, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();

    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}