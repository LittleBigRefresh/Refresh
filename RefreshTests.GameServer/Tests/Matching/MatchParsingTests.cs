using Refresh.Core.Services;

namespace RefreshTests.GameServer.Tests.Matching;

public class MatchParsingTests
{
    [Test]
    public void SerializesIpAddressFromHex() => 
        Assert.Multiple(() =>
        {
            Assert.That(MatchService.ConvertHexadecimalIpAddressToString("0x17257bc9"), Is.EqualTo("23.37.123.201"));
            Assert.That(MatchService.ConvertHexadecimalIpAddressToString("0x7f000001"), Is.EqualTo("127.0.0.1"));
            Assert.That(MatchService.ConvertHexadecimalIpAddressToString("0x04020202"), Is.EqualTo("4.2.2.2"));
            Assert.That(MatchService.ConvertHexadecimalIpAddressToString("0xffffffff"), Is.EqualTo("255.255.255.255"));
            Assert.That(MatchService.ConvertHexadecimalIpAddressToString("0x00000000"), Is.EqualTo("0.0.0.0"));
            Assert.That(MatchService.ConvertHexadecimalIpAddressToString("0x215f208,"), Is.EqualTo("0.0.0.0"));
        });

    [Test]
    public void ReplacesHexadecimalProperly()
    {
        const string original = "Lorem ipsum dolor sit amet, words 0x17257bc9 words words, 0x12345678.";
        const string expected = "Lorem ipsum dolor sit amet, words \"23.37.123.201\" words words, \"18.52.86.120\".";
        
        Assert.That(MatchService.ReplaceHexValuesInStringWithIpAddresses(original), Is.EqualTo(expected));
    }
}