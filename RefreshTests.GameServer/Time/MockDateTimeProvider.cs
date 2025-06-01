using Refresh.Common.Time;

namespace RefreshTests.GameServer.Time;

public class MockDateTimeProvider : IDateTimeProvider
{
    public long TimestampMilliseconds { get; set; }
    public long TimestampSeconds => TimestampMilliseconds / 1000;
    public long EarliestDate => 0;
    public DateTimeOffset Now => DateTimeOffset.FromUnixTimeMilliseconds(TimestampMilliseconds);
}