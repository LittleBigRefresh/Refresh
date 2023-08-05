using Refresh.GameServer.Types.Photos;

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

    [Test]
    public void CatchesInvalidFormat()
    {
        float[] floats = new float[SerializedPhotoSubject.FloatCount];
        const string bounds1 = "0.652369,0.71a0704,0.838377,0.997791";
        const string bounds2 = "0.652369,0.710704,0.838377,0.9977a91";
        
        Assert.Multiple(() =>
        {
            Assert.That(() => SerializedPhotoSubject.ParseBoundsList(bounds1, floats), Throws.TypeOf<FormatException>());
            Assert.That(() => SerializedPhotoSubject.ParseBoundsList(bounds2, floats), Throws.TypeOf<FormatException>());
        });
    }
}