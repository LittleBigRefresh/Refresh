using Refresh.GameServer.Time;

namespace RefreshTests.GameServer.Time;

public class MockDateTimeProvider : IDateTimeProvider
{
    public long TimestampMilliseconds { get; set; }
    public long TimestampSeconds => TimestampMilliseconds / 1000;
    public long EarliestDate => new DateTimeOffset(2007, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
    public DateTimeOffset Now => DateTimeOffset.FromUnixTimeMilliseconds(TimestampMilliseconds);
}