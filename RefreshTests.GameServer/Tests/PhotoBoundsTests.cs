using Refresh.GameServer.Types.UserData.Photos;

namespace RefreshTests.GameServer.Tests;

public class PhotoBoundsTests
{
    [Test]
    public void ParsesBounds()
    {
        ReadOnlySpan<char> bounds = "0.652369,0.710704,0.838377,0.997791";
        float[] floats = new float[SerializedPhotoSubject.FloatCount];
        
        SerializedPhotoSubject.ParseBoundsList(bounds, floats);

#pragma warning disable NUnit2045
        Assert.That(floats[0], Is.EqualTo(0.652369f));
        Assert.That(floats[1], Is.EqualTo(0.710704f));
        Assert.That(floats[2], Is.EqualTo(0.838377f));
        Assert.That(floats[3], Is.EqualTo(0.997791f));
#pragma warning restore NUnit2045
    }
}