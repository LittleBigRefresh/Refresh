using Refresh.Database.Helpers;

namespace RefreshTests.GameServer.Tests.Photos;

public class PhotoBoundsTests
{
    [Test]
    public void ParsesBounds()
    {
        string bounds = "0.652369,0.710704,0.838377,0.997791";
        float[] floats = PhotoHelper.ParseBoundsList(bounds);

#pragma warning disable NUnit2045
        Assert.That(floats.Length, Is.EqualTo(4));
        Assert.That(floats[0], Is.EqualTo(0.652369f));
        Assert.That(floats[1], Is.EqualTo(0.710704f));
        Assert.That(floats[2], Is.EqualTo(0.838377f));
        Assert.That(floats[3], Is.EqualTo(0.997791f));
#pragma warning restore NUnit2045
    }

    [Test]
    public void CatchesInvalidFloats()
    {
        const string bounds1 = "0.652369,0.71a0704,0.838377,0.997791";
        const string bounds2 = "0.652369,0.710704,0.838377,0.9977a91";
        
        Assert.Multiple(() =>
        {
            Assert.That(() => PhotoHelper.ParseBoundsList(bounds1), Throws.TypeOf<FormatException>());
            Assert.That(() => PhotoHelper.ParseBoundsList(bounds2), Throws.TypeOf<FormatException>());
        });
    }

    [Test]
    public void CatchesInvalidLength()
    {
        const string bounds1 = "0.652369,0.710704,0.838377,0.997791,1.0,2.0,67.0";
        const string bounds2 = "0.652369";
        
        Assert.Multiple(() =>
        {
            Assert.That(() => PhotoHelper.ParseBoundsList(bounds1), Throws.TypeOf<ArgumentException>());
            Assert.That(() => PhotoHelper.ParseBoundsList(bounds2), Throws.TypeOf<ArgumentException>());
        });
    }
}