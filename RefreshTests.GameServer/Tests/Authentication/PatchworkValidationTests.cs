using Refresh.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Authentication;

public class PatchworkValidationTests
{
    [Test, Parallelizable]
    // basic versioning tests
    [TestCase(true, "PatchworkLBP2 1.0", 1, 0)]
    [TestCase(true, "PatchworkLBP2 1.0", 0, 0)]
    [TestCase(false, "PatchworkLBP2 0.0", 1, 0)]
    [TestCase(true, "PatchworkLBP2 2.0", 1, 0)]
    [TestCase(false, "PatchworkLBP2 1.0", 2, 0)]
    [TestCase(true, "PatchworkLBP2 1.1", 1, 0)]
    [TestCase(true, "PatchworkLBP2 1.1.1", 1, 0)]
    [TestCase(true, "PatchworkLBP2 1.0.0", 1, 0)]
    // game parsing
    [TestCase(true, "PatchworkLBP1 1.0", 1, 0)]
    [TestCase(true, "PatchworkLBP2 1.0", 1, 0)]
    [TestCase(true, "PatchworkLBP3 1.0", 1, 0)]
    [TestCase(false, "PatchworkLBP4 1.0", 1, 0)]
    // test vita requiring extra data
    [TestCase(true, "PatchworkLBPV 1.0 libhttp/3.74 (PS Vita)", 1, 0)]
    [TestCase(true, "PatchworkLBPV 1.0 libhttp/4.20 (PS Vita)", 1, 0)]
    [TestCase(false, "PatchworkLBP1 1.0 libhttp/4.20 (PS Vita)", 1, 0)]
    [TestCase(false, "PatchworkLBPV 1.0 libhttp/4.20 (PS4)", 1, 0)]
    [TestCase(false, "PatchworkLBPV 1.0 asdf", 1, 0)]
    [TestCase(false, "PatchworkLBP1 1.0 asdf", 1, 0)]
    [TestCase(false, "PatchworkLBPV 1.0", 1, 0)]
    // invalid user agents
    [TestCase(false, "blah", 1, 0)]
    [TestCase(false, "Patchwork 1.0", 1, 0)]
    [TestCase(false, "MM CHTTPClient $Id: HTTPClient.cpp 36247 2009-11-24 16:17:36Z paul $", 1, 0)]
    [TestCase(false, "LBPPSP CLIENT", 1, 0)]
    [TestCase(false, "MM CHTTPClient LBP2 1.33", 1, 0)]
    [TestCase(false, "MM CHTTPClient LBP2 1.0", 1, 0)]
    public void PatchworkVersionValidates(bool expected, string userAgent, int major, int minor)
    {
        Assert.That(RequestContextExtensions.IsPatchworkVersionValid(userAgent, major, minor), Is.EqualTo(expected));
    }
}